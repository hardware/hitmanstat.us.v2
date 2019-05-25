using System.Net;
using System.Net.Http;

namespace hitmanstat.us.Framework
{
    public class DefaultHttpClientHandler : HttpClientHandler
    {
        public DefaultHttpClientHandler() =>
            AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
    }
}
