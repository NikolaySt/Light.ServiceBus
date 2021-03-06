using System;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Light.Bus.Common.NewFolder;
using Light.Common;
using Light.Common.Meta;
using Light.Common.Net.Http;

namespace Light.Client.Http
{
	public class HttpHostProxy : IHostProxy
	{
		public string ConnectionString { get; private set; }

		public string Authentication { get; private set; }

		public HttpHostProxy(
			string connectionString,
			string authentication)
		{
			if (string.IsNullOrWhiteSpace(connectionString))
				throw new ArgumentException($"'{nameof(connectionString)}' cannot be null or whitespace", nameof(connectionString));
			if (string.IsNullOrWhiteSpace(authentication))
				throw new ArgumentException($"'{nameof(authentication)}' cannot be null or whitespace", nameof(authentication));
			ConnectionString = connectionString;
			Authentication = authentication;
		}

		public HttpHostProxy(
			string connectionString)
		{
			if (string.IsNullOrWhiteSpace(connectionString))
				throw new ArgumentException($"'{nameof(connectionString)}' cannot be null or whitespace", nameof(connectionString));
			ConnectionString = connectionString;
		}

		protected async Task ExecutePostAsync(
			string requestUri,
			object item,
			CancellationToken cancellationToken = default)
		{
			await ExecutePostAsync(requestUri, item, null, cancellationToken);
		}

		protected async Task ExecutePostAsync(
			string requestUri,
			object item,
			IHttpContentFactory contentFactory,
			CancellationToken cancellationToken = default)
		{
			await ExecutePostAsync(requestUri, item, contentFactory, cancellationToken);
		}

		protected async Task<T> ExecutePostAsync<T>(
			string requestUri,
			object item,
			CancellationToken cancellationToken = default)
		{
			return await ExecutePostAsync<T>(requestUri, item, null, cancellationToken);
		}

		protected async Task<T> ExecutePostAsync<T>(
			string requestUri,
			object item,
			IHttpContentFactory contentFactory,
			CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(requestUri)) throw new ArgumentException(nameof(requestUri));
			if (item == null) throw new ArgumentException(nameof(item));

			HttpExtendedClient client = null;

			try
			{
				client = GetHttpClient();

				var response = await client.PostAsync<Response<T>>(
					requestUri,
					contentFactory ?? new JsonContentFactory(),
					item,
					cancellationToken);

				if (!response.IsSuccess) throw ExceptionBuilder.GetInstance(response);

				return response.Result;
			}
			finally
			{
				client?.Dispose();
			}
		}

		protected string ProcessUri(string resourceUri)
		{
			if (string.IsNullOrWhiteSpace(resourceUri)) throw new ArgumentException(nameof(resourceUri));

			return string.Format("/{0}", resourceUri);
		}

		protected string ProcessUri(string resourceUri, string targetAppRealmId)
		{
			if (string.IsNullOrWhiteSpace(resourceUri)) throw new ArgumentException(nameof(resourceUri));

			var bld = new StringBuilder();

			bld.AppendFormat("/{0}", resourceUri);

			if (!string.IsNullOrWhiteSpace(targetAppRealmId)) bld.AppendFormat("?targetAppRealmId={0}", targetAppRealmId);

			return bld.ToString();
		}

		protected HttpExtendedClient GetHttpClient()
		{
			var client = new HttpExtendedClient
			{
				BaseAddress = new Uri(ConnectionString)
			};

			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Authentication);
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

			//client.RetryCount = ProxySettings.Instance.RetryCount;
			//client.IsVerbose = ProxySettings.Instance.IsVerbose;

			return client;
		}

		public async Task ExecuteAsync(
			Event @event,
			string channelId = null,
			CancellationToken cancellationToken = default)
		{
			if (@event == null) throw new ArgumentException(nameof(@event));

			var requestUri = GetRequestOperationUri();

			var genericOperation = GetGenericOperation(@event);

			genericOperation.ChannelId = channelId;

			await ExecutePostAsync(requestUri, genericOperation, cancellationToken);
		}

		public async Task ExecuteAsync(
			Operation operation,
			CancellationToken cancellationToken = default)
		{
			if (operation == null) throw new ArgumentException(nameof(operation));

			var requestUri = GetRequestOperationUri();

			var genericOperation = GetGenericOperation(operation);

			await ExecutePostAsync(requestUri, genericOperation, cancellationToken);
		}

		public async Task<T> ExecuteAsync<T>(
			Operation<T> operation,
			CancellationToken cancellationToken = default)
		{
			if (operation == null) throw new ArgumentException(nameof(operation));

			var requestUri = GetRequestOperationUri();

			var genericOperation = GetGenericOperation(operation);

			var result = await ExecutePostAsync<T>(requestUri, genericOperation, cancellationToken);

			return result;
		}

		protected virtual string GetRequestOperationUri()
		{
			return "/execute";
		}

		private GenericOperation GetGenericOperation(
			IOperation operation)
		{
			if (operation == null) throw new ArgumentException(nameof(operation));

			return new GenericOperation
			{
				Authentication = string.IsNullOrWhiteSpace(operation.Authentication) ? Authentication : operation.Authentication,
				ContextNamespace = operation.ContextNamespace,
				ContextOperation = operation.ContextOperation,
				PayloadTypeName = operation.GetType().FullName,
				Payload = JsonConvert.ToJson(operation)
			};
		}

		private GenericOperation GetGenericOperation(
			Event operation)
		{
			if (operation == null) throw new ArgumentException(nameof(operation));

			return new GenericOperation
			{
				Authentication = Authentication,
				ContextNamespace = operation.ContextNamespace,
				ContextOperation = operation.ContextOperation,
				PayloadTypeName = operation.GetType().FullName,
				Payload = JsonConvert.ToJson(operation)
			};
		}

		private GenericOperation GetGenericOperation<T>(
			IOperation<T> operation)
		{
			if (operation == null) throw new ArgumentException(nameof(operation));

			return new GenericOperation
			{
				Authentication = string.IsNullOrWhiteSpace(operation.Authentication) ? Authentication : operation.Authentication,
				ContextNamespace = operation.ContextNamespace,
				ContextOperation = operation.ContextOperation,
				PayloadTypeName = operation.GetType().FullName,
				Payload = JsonConvert.ToJson(operation)
			};
		}
	}
}