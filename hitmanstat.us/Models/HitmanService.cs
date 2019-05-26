using System.ComponentModel.DataAnnotations;

namespace hitmanstat.us.Models
{
    public class HitmanService
    {
        public string Name { get; set; }
        public string Node { get; set; }
        public HitmanServiceHealth Health { get; set; }
    }

    public enum HitmanServiceHealth
    {
        [Display(Name = "unknown")]
        Unknown,
        [Display(Name = "down")]
        Down,
        [Display(Name = "maintenance")]
        Maintenance,
        Slow,
        Healthy
    }
}
