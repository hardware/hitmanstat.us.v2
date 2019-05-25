namespace hitmanstat.us.Options
{
    public class PolicyOptions
    {
        public CircuitBreakerPolicyOptions HttpCircuitBreaker { get; set; }
        public RetryPolicyOptions HttpRetry { get; set; }
        public TimeoutPolicyOptions HttpTimeout { get; set; }
    }
}
