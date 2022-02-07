﻿using System;
using System.ComponentModel.DataAnnotations.Schema;

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
        public int H3epic { get; set; }
        public int H3steam { get; set; }
        public int H3xb { get; set; }
        public int H3ps { get; set; }
        public int H3st { get; set; }
        public int H3sw { get; set; }
        [Column(TypeName = "date")]
        public DateTime Date { get; set; }
    }
}
