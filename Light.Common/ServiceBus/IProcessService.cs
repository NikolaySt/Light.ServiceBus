using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Light.Bus.Common
{
	public interface IProcessService
	{
		Task RequestDelegate(HttpContext context);

		IWebHost Host { get; set; }

		string Endpoint { get; set; }
	}
}