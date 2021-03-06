using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Light.Bus.Common
{
	public class ServiceHostStartup
	{
		public IProcessService Process { get; }

		public ServiceHostStartup(
			IConfiguration configuration,
			IProcessService process)
		{
			Configuration = configuration;
			Process = process;
		}

		public IConfiguration Configuration { get; }

		public void ConfigureServices(IServiceCollection services)
		{
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(
			IApplicationBuilder app,
			Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
		{
			app.UseWebSockets();
			app.Run(Process.RequestDelegate);
		}
	}
}