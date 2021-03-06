using Light.Client.Http;

namespace Light.Bus.Common
{
	public abstract class ServiceComponent : IServiceComponent
	{
		public IHostProxy Host { get; set; }
	}

	public interface IServiceComponent
	{
		IHostProxy Host { get; set; }
	}
}