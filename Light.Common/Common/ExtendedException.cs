using System;

namespace Light.Common.Meta
{
	public abstract class ExtendedException : Exception
	{
		protected ExtendedException(int code, string message)
			: base(message)
		{
			Code = code;
		}

		protected ExtendedException(int code, string message, ExtendedException innerException)
			: base(message, innerException)
		{
			Code = code;
		}

		public int Code { get; }
	}
}