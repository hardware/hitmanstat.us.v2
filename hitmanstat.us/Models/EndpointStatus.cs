using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace hitmanstat.us.Models
{
    public class EndpointStatus
    {
        public string Name { get; set; }
        public string Status { get; set; }
        public EndpointState State { get; set; } = EndpointState.Down;

        public EndpointStatus() {}
    }

    public class EndpointStatusException : EndpointStatus
    {
        public string Message { get; set; }

        public EndpointStatusException(string name) => Name = name;
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum EndpointState
    {
        [Display(Name = "up")]
        Up,
        [Display(Name = "down")]
        Down,
        [Display(Name = "maintenance")]
        Maintenance,
        [Display(Name = "unknown")]
        Unknown
    }
}
