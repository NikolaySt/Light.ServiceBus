using System.Threading;
using System.Threading.Tasks;
using Light.Common;

namespace Light.Client.Http
{
	public interface IHostProxy
	{
		Task ExecuteAsync(
			Event @event,
			string channelId = null,
			CancellationToken cancellationToken = default);

		Task ExecuteAsync(
			Operation operation,
			CancellationToken cancellationToken = default);

		Task<T> ExecuteAsync<T>(
			Operation<T> operation,
			CancellationToken cancellationToken = default);
	}
}