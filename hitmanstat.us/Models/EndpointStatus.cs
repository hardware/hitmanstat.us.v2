using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace hitmanstat.us.Models
{
    public class EndpointStatus
    {
        public EndpointName Name { get; set; }
        public EndpointState State { get; set; } = EndpointState.Down;
        public string Status { get; set; }

        public EndpointStatus() {}
    }

    public class EndpointStatusException : EndpointStatus
    {
        public string Message { get; set; }

        public EndpointStatusException(EndpointName name) => Name = name;
    }

    public enum EndpointName
    {
        [Display(Name = "HITMAN AUTHENTICATION")]
        HitmanAuthentication,
        [Display(Name = "HITMAN FORUM")]
        HitmanForum
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
