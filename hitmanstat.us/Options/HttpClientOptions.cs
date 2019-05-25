using System;

namespace hitmanstat.us.Options
{
    public class HttpClientOptions
    {
        public Uri BaseAddress { get; set; }
        public TimeSpan Timeout { get; set; }
    }
}
