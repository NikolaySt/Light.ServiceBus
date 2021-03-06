using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Light.Common.Net.Http
{
	public class HttpExtendedClient : HttpClient
	{
		public int RetryCount { get; set; }

		public bool IsVerbose { get; set; }

		public HttpExtendedClient()
		{
		}

		public HttpExtendedClient(HttpMessageHandler handler)
			: base(handler)
		{
		}

		public async Task<T> PostAsync<T>(
			string requestUri,
			IHttpContentFactory contentFactory,
			object input,
			CancellationToken cancellationToken)
		{
			var httpResponse = await ExecuteAsync(
					HttpMethod.Post,
					requestUri,
					contentFactory,
					input,
					HttpCompletionOption.ResponseContentRead,
					cancellationToken);

			var json = await httpResponse.Content.ReadAsStringAsync();

			return JsonConvert.ToObject<T>(json);
		}

		public async Task<HttpResponseMessage> ExecuteAsync(
			HttpMethod method,
			string requestUri,
			IHttpContentFactory contentFactory,
			object input,
			HttpCompletionOption completionOption,
			CancellationToken cancellationToken)
		{
			var retry = 0;
			bool isConnectivityFailure;
			HttpResponseMessage httpResponse = null;
			Exception exception = null;
			do
			{
				try
				{
					exception = null;

					HttpRequestMessage request;
					if (!string.IsNullOrWhiteSpace(requestUri))
					{
						if (BaseAddress != null)
						{
							var uri = new Uri(BaseAddress, requestUri);

							request = new HttpRequestMessage(method, uri);
						}
						else
						{
							request = new HttpRequestMessage(method, requestUri);
						}
					}
					else
					{
						request = new HttpRequestMessage(method, BaseAddress);
					}

					if (contentFactory != null && input != null) request.Content = contentFactory.GetContent(input);

					var timestamp1 = DateTime.UtcNow;
					string requestContent = null;

					if (IsVerbose)
					{
						if (request.Content != null) requestContent = await request.Content.ReadAsStringAsync();
					}

					httpResponse = await SendAsync(request, completionOption, cancellationToken).ConfigureAwait(false);

					if (IsVerbose)
					{
						var timestamp2 = DateTime.UtcNow;

						var bld = new StringBuilder();
						bld.AppendLine("=================================================================");
						bld.AppendFormat("Starting {0}{1} : {2}\n", BaseAddress, requestUri, timestamp1);
						if (DefaultRequestHeaders.Authorization != null) bld.AppendFormat("Authorization : {0}\n", DefaultRequestHeaders.Authorization);
						if (!string.IsNullOrWhiteSpace(requestContent)) bld.AppendFormat("Request Body: {0}\n", requestContent);
						bld.AppendFormat("Response time : {0}\n", (timestamp2 - timestamp1).TotalMilliseconds);
						bld.AppendFormat("Status: {0}, {1}\n", httpResponse.StatusCode, httpResponse.ReasonPhrase);

						if (httpResponse.Content != null)
						{
							var content = await httpResponse.Content.ReadAsStringAsync();
							bld.AppendFormat("Response Body: {0}\n", content);
						}

						Debug.WriteLine(bld.ToString());
					}

					isConnectivityFailure = false;
				}
				catch (HttpRequestException ex)
				{
					exception = ex;
					isConnectivityFailure = true;
					await Task.Delay(1000, cancellationToken);
				}
				catch (TaskCanceledException ex)
				{
					exception = ex;
					isConnectivityFailure = true;
					await Task.Delay(1000, cancellationToken);
				}
				retry++;
			} while (isConnectivityFailure && retry < RetryCount);

			if (exception != null) throw exception;

			return httpResponse;
		}
	}
}