using System;
using System.Threading.Tasks;
using Light.Bus.Common;
using Light.Client.Http;
using Light.Common;

namespace Light.Bus2
{
	internal class Program
	{
		private static HttpHostProxy serverProxy;

		private static ProcessService ProcessService;

		private static string channelId;

		private static async Task Main(string[] args)
		{
			Console.WriteLine("Hello World! Bus!");
			channelId = Guid.NewGuid().ToString().Replace("-", "");
			serverProxy = new HttpHostProxy($"http://localhost:4641");

			ProcessService = new ProcessService();
			ProcessService.Endpoint = $"http://localhost:4642";

			Task.Factory.StartNew(async () =>
			{
				await Task.Delay(TimeSpan.FromSeconds(10));
				await SubscribeForEvents();
			});

			//await new ServerHost(new ProcessService()).RunAsync(4642);
		}

		private static async Task SubscribeForEvents()
		{
			await ProcessService.Subscribe<IEventContract>(serverProxy, channelId, HandleEventMessage);
		}

		private static void HandleEventMessage(
			string value,
			IEventContract superEvent)
		{
			Console.WriteLine("Succeeded HandleEventMessage");
			ProcessService.Unsubscribe(serverProxy, channelId);
		}
	}
}