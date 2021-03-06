using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Light.Bus.Common.NewFolder;
using Light.Common;
using Light.Contracts;
using Light.Client.Http;

namespace Light.Bus.Common
{
	public class ProcessService : IProcessService
	{
		private static readonly ConcurrentDictionary<string, Delegate> SubscriptionDelegates =
			new ConcurrentDictionary<string, Delegate>();

		public ProcessService()
		{
		}

		public IWebHost Host { get; set; }

		public string Endpoint { get; set; }

		public async Task RequestDelegate(HttpContext context)
		{
			if (context.WebSockets.IsWebSocketRequest)
			{
				using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
				await WebSocketEchoResponse(context, webSocket);
				return;
			}

			if (context.Request.Method == "POST")
			{
				using var memoryStream = new MemoryStream();
				context.Request.Body.CopyTo(memoryStream);
				var buffer = memoryStream.ToArray();

				var json = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
				var superOperation = JsonConvert.ToObject<GenericOperation>(json);
				var operation = ServicePersister.GetOperation(superOperation);
				var services = ServicePersister.ResolveEvent(operation.GetType());

				object result = default;

				if (!string.IsNullOrWhiteSpace(superOperation.ChannelId))
				{
					foreach (var item in SubscriptionDelegates.Where(it => it.Key == superOperation.ChannelId))
					{
						item.Value.DynamicInvoke(new object[] { item.Key, operation });
					}
				}

				foreach (var serviceDetails in services)
				{
					var service = Host.Services.GetService(serviceDetails.Item1);

					if (operation is Event)
						ServicePersister.InvokeEventHandlersAsync(service as ServiceComponent, operation);
					else
					{
						result = await ServicePersister.InvokeServiceHandlerAsync(service as ServiceComponent, operation);
						break;
					}
				}

				if (result == null)
					result = new Response(Outcomes.Success);

				json = JsonConvert.ToJson(result);
				var content = Encoding.UTF8.GetBytes(json);
				await context.Response.Body.WriteAsync(content, 0, content.Length);
			}
		}

		public async Task Subscribe<DataType>(
			HttpHostProxy proxy,
			string channeId,
			Action<string, DataType> @delegate)
			where DataType : IEventContract
		{
			if (proxy is null)
				throw new ArgumentNullException(nameof(proxy));
			if (string.IsNullOrEmpty(channeId))
				throw new ArgumentException($"'{nameof(channeId)}' cannot be null or empty", nameof(channeId));
			if (@delegate is null)
				throw new ArgumentNullException(nameof(@delegate));

			var subscribeMsg = new Subscribe()
			{
				ChannelId = channeId,
				EndPoint = Endpoint,
			};

			await proxy.ExecuteAsync(subscribeMsg);

			SubscriptionDelegates.TryAdd(channeId, @delegate);

			Console.WriteLine($"Succeeded Subscribe");
		}

		public async Task Unsubscribe(
			HttpHostProxy proxy,
			string channeId)
		{
			if (string.IsNullOrWhiteSpace(channeId))
				throw new ArgumentException($"'{nameof(channeId)}' cannot be null or whitespace", nameof(channeId));

			var subscribeMsg = new Unsubscribe()
			{
				ChannelId = channeId,
				EndPoint = Endpoint,
			};

			await proxy.ExecuteAsync(subscribeMsg);

			if (!SubscriptionDelegates.TryRemove(channeId, out Delegate _))
				Console.WriteLine($"Failed to Unsubscribe");
			else
				Console.WriteLine($"Succeeded to Unsubscribe");
		}

		private async Task WebSocketEchoResponse(
			HttpContext context, WebSocket webSocket)
		{
			var buffer = new byte[1024 * 4];
			var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
			while (!result.CloseStatus.HasValue)
			{
				await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);

				result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
			}
			await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
		}
	}
}