namespace Light.Common
{
	public interface IContract
	{
		string ComponentName { get; set; }

		string ComponentVersion { get; set; }

		string MessageId { get; set; }

		string ContextNamespace { get; set; }

		string ContextOperation { get; set; }
	}
}