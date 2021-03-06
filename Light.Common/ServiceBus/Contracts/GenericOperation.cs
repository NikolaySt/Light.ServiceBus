using Light.Common;

namespace Light.Bus.Common.NewFolder
{
	public class GenericOperation : Operation
	{
		public string Payload { get; set; }

		public string PayloadTypeName { get; set; }

		public string ChannelId { get; set; }
	}

	public class GenericOperation<T> : Operation<T>
	{
		public string Payload { get; set; }

		public string PayloadTypeName { get; set; }

		public string ChannelId { get; set; }
	}
}