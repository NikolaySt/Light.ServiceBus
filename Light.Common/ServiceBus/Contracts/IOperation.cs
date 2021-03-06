using System.Collections.Generic;

namespace Light.Common
{
	public interface IOperation : IContract
	{
		string Authentication { get; set; }

		bool HasAuthentication { get; }

		Dictionary<string, object> ExecutionContext { get; set; }

		T GetContextValue<T>(string key);

		void CopyTo(IOperation operation);
	}

	public interface IOperation<T> : IOperation
	{
	}
}