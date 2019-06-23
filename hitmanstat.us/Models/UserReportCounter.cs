using System;
using System.ComponentModel.DataAnnotations;

namespace hitmanstat.us.Models
{
    public class UserReportCounter
    {
        public int ID { get; set; }
        public int H1pc { get; set; }
        public int H1xb { get; set; }
        public int H1ps { get; set; }
        public int H2pc { get; set; }
        public int H2xb { get; set; }
        public int H2ps { get; set; }
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }
    }
}
