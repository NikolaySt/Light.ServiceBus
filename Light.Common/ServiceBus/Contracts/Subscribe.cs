using Light.Common;

namespace Light.Contracts
{
	public class Subscribe : Operation
	{
		public Subscribe()
		{
		}

		public string EndPoint { get; set; }

		public string ChannelId { get; set; }
	}

	public class Unsubscribe : Operation
	{
		public Unsubscribe()
		{
		}

		public string EndPoint { get; set; }

		public string ChannelId { get; set; }
	}
}