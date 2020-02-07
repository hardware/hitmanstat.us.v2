using System.ComponentModel.DataAnnotations;

namespace hitmanstat.us.Models
{
    public class HitmanService
    {
        public string Name { get; set; }
        public string Node { get; set; }
        public string Ref { get; set; }
        public HitmanServiceHealth Health { get; set; }
    }

    /*
     * Note : 
     * Health members with display attribute will be
     * stored as event in database. This bahavior 
     * allows to exclude slow and healthy to be 
     * registered.
     */
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
