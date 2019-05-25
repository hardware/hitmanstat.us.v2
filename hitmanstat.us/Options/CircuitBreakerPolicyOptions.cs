using System;

namespace hitmanstat.us.Options
{
    public class CircuitBreakerPolicyOptions
    {
        public double FailureThreshold { get; set; } = 0.5;
        public TimeSpan SamplingDuration { get; set; } = TimeSpan.FromSeconds(30);
        public int MinimumThroughput { get; set; } = 10;
        public TimeSpan DurationOfBreak { get; set; } = TimeSpan.FromSeconds(60);
    }
}
