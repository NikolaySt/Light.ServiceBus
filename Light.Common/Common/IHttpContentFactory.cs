using System.Net.Http;

namespace Light.Common.Net.Http
{
	public interface IHttpContentFactory
	{
		HttpContent GetContent(object input);
	}
}