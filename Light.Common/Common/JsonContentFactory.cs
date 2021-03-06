using System;
using System.Net.Http;
using System.Text;

namespace Light.Common.Net.Http
{
	public class JsonContentFactory : IHttpContentFactory
	{
		private readonly bool _applyTypeInfo;

		private readonly Encoding _encoding = Encoding.UTF8;

		public JsonContentFactory()
		{
			_encoding = Encoding.UTF8;
			_applyTypeInfo = false;
		}

		public JsonContentFactory(Encoding encoding)
		{
			if (encoding == null) throw new ArgumentException(nameof(encoding));

			_encoding = encoding;
			_applyTypeInfo = false;
		}

		public JsonContentFactory(bool applyTypeInfo)
		{
			_encoding = Encoding.Unicode;
			_applyTypeInfo = applyTypeInfo;
		}

		public JsonContentFactory(Encoding encoding, bool applyTypeInfo)
		{
			if (encoding == null) throw new ArgumentException(nameof(encoding));

			_encoding = encoding;
			_applyTypeInfo = applyTypeInfo;
		}

		public HttpContent GetContent(object input)
		{
			if (input == null) throw new ArgumentException(nameof(input));

			var json = JsonConvert.ToJson(input, _applyTypeInfo);

			return new StringContent(json, _encoding, "application/json");
		}
	}
}