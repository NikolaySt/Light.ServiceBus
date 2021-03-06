using System;
using System.Linq;
using System.Threading.Tasks;
using Light.Bus.Common;
using Light.Client.Http;
using Light.Contracts;
using Light.Services;

namespace Light.Bus
{
	internal class Program
	{
		private static async Task Main(string[] args)
		{
			var type = typeof(PingComponent);

			Console.WriteLine("Hello World! Bus!");

			Task.Factory.StartNew(async () =>
			{
				await Task.Delay(TimeSpan.FromSeconds(20));
				await RunTestAsync();
			});

			var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(it => it.GetName().Name.StartsWith("Light")).ToList();
			var components = ServicePersister.GetServiceTypes(assemblies).ToList();

			await new ServerHost()
				.Listen(4641)
				.RegisterComponents(components)
				.RunAsync();
		}

		private static async Task RunTestAsync()
		{
			try
			{
				var proxy = new HttpHostProxy($"http://localhost:4641");

				var msg = new PingOperation()
				{
				};
				await proxy.ExecuteAsync(msg);
				Console.WriteLine($"Succeeded PingOperation");

				var @event = new AppDeleted()
				{
				};
				await proxy.ExecuteAsync(@event);
				Console.WriteLine($"Succeeded Event");
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}
	}
}