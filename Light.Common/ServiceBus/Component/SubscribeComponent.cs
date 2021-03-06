using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Light.Bus.Common;
using Light.Bus.Common.NewFolder;
using Light.Client.Http;
using Light.Common;
using Light.Contracts;

namespace Light.Host
{
	public class SubscribeComponent : ServiceComponent
	{
		private List<Subscribe> Subscribed { get; set; }

		public SubscribeComponent()
		{
			Subscribed = new List<Subscribe>();
		}

		[MessageHandler]
		public async Task<Response> HandleAsync(Subscribe operation)
		{
			Subscribed.Add(operation);
			Console.WriteLine($"Succeeded Subscribe {operation.ChannelId}");
			await Task.CompletedTask;
			return new Response(Outcomes.Success);
		}

		[MessageHandler]
		public async Task<Response> HandleAsync(Unsubscribe operation)
		{
			Subscribed.RemoveAll(it => it.ChannelId == operation.ChannelId && it.EndPoint == operation.EndPoint);
			Console.WriteLine($"Succeeded Unsubscribe {operation.ChannelId}");
			await Task.CompletedTask;
			return new Response(Outcomes.Success);
		}

		[MessageHandler]
		public async Task HandleAsync(Event @event)
		{
			foreach (var operation in Subscribed)
			{
				try
				{
					var proxy = new HttpHostProxy(operation.EndPoint);
					await proxy.ExecuteAsync(@event, operation.ChannelId);
					Console.WriteLine($"Succeeded Send {operation.ChannelId}");
				}
				catch { /* ignore*/ }
			}

			await Task.CompletedTask;
		}
	}
}