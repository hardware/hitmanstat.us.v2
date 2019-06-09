using System;
using System.ComponentModel.DataAnnotations;

namespace hitmanstat.us.Models
{
    public class Event
    {
        public int ID { get; set; }
        public string Service { get; set; }
        public string Status { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MMM d, yyyy HH:mm}")]
        public DateTime Date { get; set; }
    }
}
