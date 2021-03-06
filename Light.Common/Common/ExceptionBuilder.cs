using System;

namespace Light.Common.Meta
{
	public static class ExceptionBuilder
	{
		public static ExtendedException GetInstance(
			IResponse response)
		{
			if (response == null) throw new ArgumentException(nameof(response));

			var code = response.Code;

			return Get(code, response.Message, null);
		}

		private static ExtendedException Get(
			int code,
			string message,
			ExtendedException innerException)
		{
			throw new NotImplementedException($"ExceptionBuilder.Get code: {code}");
		}
	}
}