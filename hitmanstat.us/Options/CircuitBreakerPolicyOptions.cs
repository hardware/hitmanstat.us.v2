using System;

namespace hitmanstat.us.Options
{
    public class CircuitBreakerPolicyOptions
    {
        public TimeSpan DurationOfBreak { get; set; } = TimeSpan.FromMinutes(2);

        public int ExceptionsAllowedBeforeBreaking { get; set; } = 20;
    }
}
