using System;

namespace hitmanstat.us.Options
{
    public class TimeoutPolicyOptions
    {
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);
    }
}
