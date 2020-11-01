using System;
using System.ComponentModel.DataAnnotations;

namespace hitmanstat.us.Models
{
    public class Event
    {
        public int ID { get; set; }
        [Required]
        public string Service { get; set; }
        [Required]
        public string State { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MMM d, HH:mm}")]
        public DateTime Date { get; set; }
    }
}
