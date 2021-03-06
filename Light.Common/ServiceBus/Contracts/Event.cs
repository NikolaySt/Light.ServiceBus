namespace Light.Common
{
	public abstract class Event : Contract, IEventContract
	{
		protected Event(
			string contextNamespace,
			string contextOperation)
			: base(contextNamespace, contextOperation)
		{
		}
	}
}