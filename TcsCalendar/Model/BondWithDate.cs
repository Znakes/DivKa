using System;

namespace TcsCalendar.Model
{
    public class BondWithDate
    {
        public string Name { get; set; }
        public string Ticker { get; set; }
        public int Lots { get; set; }
        public DateTime Date { get; set; }
        public double Value { get; set; }
        public string Currency { get; set; }
    }
}
