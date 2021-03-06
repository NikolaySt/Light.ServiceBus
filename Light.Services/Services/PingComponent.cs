using System.Threading.Tasks;
using Light.Bus.Common;
using Light.Bus.Common.NewFolder;
using Light.Common;
using Light.Contracts;

namespace Light.Services
{
	public class PingComponent : ServiceComponent
	{
		public PingComponent()
		{
		}

		[MessageHandler]
		public async Task<Response> HandleAsync(PingOperation operation)
		{
			await Task.CompletedTask;
			return new Response(Outcomes.Success);
		}
	}
}