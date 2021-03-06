using Light.Common;

namespace Light.Contracts
{
	public class AppDeleted : Event
	{
		public AppDeleted()
			: base("App", "Delete")
		{
		}

		public string AppId { get; set; }
	}
}