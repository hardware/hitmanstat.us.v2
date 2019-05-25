namespace hitmanstat.us.Models
{
    public class ErrorViewModel
    {
        public string RequestId { get; set; }
        public int? ErrorCode { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
        public bool ShowErrorCode => ErrorCode.HasValue;
    }
}