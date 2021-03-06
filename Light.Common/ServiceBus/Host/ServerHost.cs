using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Light.Bus.Common
{
	public class ServerHost
	{
		public readonly CancellationTokenSource CancelSource =
			new CancellationTokenSource();

		public IWebHost Host { get; private set; }

		public IProcessService ProcessService { get; private set; }

		public int Port { get; private set; }

		public IList<Type> Components { get; private set; }

		public ServerHost Listen(int port)
		{
			Port = port;
			return this;
		}

		public ServerHost RegisterComponents(IList<Type> components)
		{
			Components = components ?? new List<Type>();
			return this;
		}

		public async Task RunAsync()
		{
			var localEndPoint = DetermineEndPoint(Port);

			ProcessService = new ProcessService();

			var host = new WebHostBuilder()
				.UseKestrel()
				.ConfigureServices(it =>
				{
					it.AddSingleton(ProcessService);
					Components.ForEach(s => it.AddSingleton(Activator.CreateInstance(s)));
				})
				.ConfigureKestrel(serverOptions =>
				{
					serverOptions.Listen(localEndPoint.Address, localEndPoint.Port);
				})
				.UseStartup<ServiceHostStartup>()
				.Build();

			Host = host;

			ProcessService.Host = host;

			await host.RunAsync(CancelSource.Token);
		}

		public void StopAsync()
		{
			CancelSource.Cancel();
		}

		private static IPEndPoint DetermineEndPoint(int port)
		{
			var ipAddress = IPAddress.Loopback;

			var localEndPoint = new IPEndPoint(ipAddress, port);

			Console.WriteLine($"Open for connections at: {localEndPoint}");

			return localEndPoint;
		}
	}
}