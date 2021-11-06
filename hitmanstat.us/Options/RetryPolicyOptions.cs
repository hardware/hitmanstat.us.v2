namespace hitmanstat.us.Options
{
    public class RetryPolicyOptions
    {
        public int Count { get; set; } = 2;
        public int BackoffPower { get; set; } = 2;
    }
}
