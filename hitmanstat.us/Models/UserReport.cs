using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;

namespace hitmanstat.us.Models
{
    public class UserReport
    {
        public int ID { get; set; }
        [Required, MinLength(4), MaxLength(16)]
        public byte[] IPAddressBytes { get; set; }
        [Required]
        // 128 bit x64 MurmurHash3
        [StringLength(32)]
        [RegularExpression("^[a-z0-9]$")] 
        public string Fingerprint { get; set; }
        [Required]
        public string Service { get; set; }
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        [NotMapped]
        public IPAddress IPAddress
        {
            get { return new IPAddress(IPAddressBytes); }
            set { IPAddressBytes = value.GetAddressBytes(); }
        }
    }
}
