using System;
using System.Collections.Generic;

namespace Light.Common
{
	public abstract class Operation : Contract, IOperation
	{
		public string Authentication { get; set; }

		public bool HasAuthentication => !string.IsNullOrWhiteSpace(Authentication);

		public Dictionary<string, object> ExecutionContext { get; set; }

		protected Operation()
		{
			ExecutionContext = new Dictionary<string, object>();
		}

		protected Operation(string contextOperation)
			: base(contextOperation)
		{
			ExecutionContext = new Dictionary<string, object>();
		}

		protected Operation(string contextNamespace, string contextOperation)
			: base(contextNamespace, contextOperation)
		{
			ExecutionContext = new Dictionary<string, object>();
		}

		public T GetContextValue<T>(string key)
		{
			if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException(nameof(key));

			if (ExecutionContext == null) return default;

			if (!ExecutionContext.ContainsKey(key)) return default;

			var value = ExecutionContext[key];
			if (value == null) return default;

			if (value is string)
			{
				if (typeof(T).IsAssignableFrom(typeof(string))) return (T)value;

				return JsonConvert.ToObject<T>(value as string);
			}

			var json = JsonConvert.ToJson(value);

			return JsonConvert.ToObject<T>(json);
		}

		public virtual void CopyTo(IOperation operation)
		{
			if (!(operation is Operation target)) throw new ArgumentException(nameof(operation));

			target.Authentication = Authentication;

			target.ExecutionContext = null;
			if (ExecutionContext != null)
			{
				target.ExecutionContext = new Dictionary<string, object>();

				var json = JsonConvert.ToJson(target.ExecutionContext, true);
				target.ExecutionContext = JsonConvert.ToObject<Dictionary<string, object>>(json);
			}
		}
	}

	public abstract class Operation<T> : Operation, IOperation<T>
	{
		protected Operation()
		{
		}

		protected Operation(string contextOperation)
			: base(contextOperation)
		{
		}

		protected Operation(string contextNamespace, string contextOperation)
			: base(contextNamespace, contextOperation)
		{
		}
	}
}