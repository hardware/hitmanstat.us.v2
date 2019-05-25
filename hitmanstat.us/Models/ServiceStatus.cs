using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace hitmanstat.us.Models
{
    public class ServiceStatus
    {
        public string Status { get; set; }
        public ServiceState State { get; set; } = ServiceState.Down;
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ServiceState
    {
        Up,
        Down,
        Maintenance
    }
}
