using System.ComponentModel.DataAnnotations;

namespace hitmanstat.us.Models
{
    public class SubmitReport
    {
        [Required]
        [RegularExpression(@"^(h1pc|h1xb|h1ps|h2pc|h2xb|h2ps)$")]
        public string Reference { get; set; }

        [Required]
        [RegularExpression(@"^[a-z0-9]{32}$")]
        public string Fingerprint { get; set; }

        [Required]
        public string State { get; set; }

        [Required]
        public string RecaptchaToken { get; set; }

        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }
    }
}
