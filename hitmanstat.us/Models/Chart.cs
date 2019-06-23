using System.Collections.Generic;

namespace hitmanstat.us.Models
{
    public class Chart
    {
        public IEnumerable<string> Categories { get; set; }
        public IEnumerable<ChartSerie> Series { get; set; }
    }

    public class ChartSerie
    {
        public string Name { get; set; }
        public IEnumerable<int> Data { get; set; }
    }
}
