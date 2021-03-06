using System;
using System.Reflection;

namespace Light.Common
{
	public abstract class Contract : IContract
	{
		private static readonly string ComponentNameCache = "";

		private static readonly string ComponentVersionCache = "";

		static Contract()
		{
			var entry = Assembly.GetEntryAssembly();
			if (entry == null) return;

			var assemblyName = entry.GetName();

			ComponentNameCache = assemblyName.FullName;
			ComponentVersionCache = assemblyName.Version?.ToString();
		}

		protected Contract()
		{
			ComponentName = ComponentNameCache;
			ComponentVersion = ComponentVersionCache;
			MessageId = Guid.NewGuid().ToString();
		}

		protected Contract(
			string contextOperation)
			: this()
		{
			if (string.IsNullOrWhiteSpace(contextOperation)) throw new ArgumentException(nameof(contextOperation));

			ContextOperation = contextOperation;
		}

		protected Contract(
			string contextNamespace,
			string contextOperation)
			: this(contextOperation)
		{
			if (string.IsNullOrWhiteSpace(contextNamespace)) throw new ArgumentException(nameof(contextNamespace));

			ContextNamespace = contextNamespace;
		}

		public string ComponentName { get; set; }

		public string ComponentVersion { get; set; }

		public string MessageId { get; set; }

		public string ContextNamespace { get; set; }

		public string ContextOperation { get; set; }
	}
}