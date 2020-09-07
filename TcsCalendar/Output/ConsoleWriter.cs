using System;
using System.Collections.Generic;
using System.Linq;
using TcsCalendar.Model;

namespace TcsCalendar.Output
{
    public class ConsoleWriter
    {
        public void PrintText(string text)
        {
            Console.WriteLine(text);
        }

        public void PrintBonds(IEnumerable<BondWithDate> bonds)
        {
            Console.WriteLine();
            Console.Write(TableParser.ToStringTable(bonds.OrderBy(x => x.Date),
                    columnHeaders: new [] { "Date", "Coupon", "Lots", "Total", "Name" },
                    bond => bond.Date.ToString("dd MMM yyyy"),
                    bond => bond.Value.ToString("#.##"),
                    bond => bond.Lots,
                    bond => (bond.Value * bond.Lots).ToString("#.##"),
                    bond => bond.Name));
        }
    }
}
