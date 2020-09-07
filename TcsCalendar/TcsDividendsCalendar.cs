using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TcsCalendar.Model;
using TcsCalendar.Output;
using TcsCalendar.Providers;
using static Tinkoff.Trading.OpenApi.Models.Portfolio;

namespace TcsCalendar
{
    public class TcsDividendsCalendar
    {
        private readonly TcsProvider tcsProvider;
        private readonly MoexProvider moexProvider;
        private readonly ConsoleWriter consoleWriter;

        /// <summary>
        /// if you have more than 1 acoount(f.e. Broker and IIS), merge similar bonds into one
        /// </summary>
        public bool AggregateByAccounts { get; set; } = true;

        /// <summary>
        /// Add row "Total for XXXX year" in the end of each year
        /// </summary>
        public bool DisplayYearResults { get; set; } = true;

        /// <summary>
        /// Last day of calculated payments
        /// </summary>
        public DateTime LastDayOfCalendar { get; set; } = DateTime.UtcNow.AddYears(1);

        public TcsDividendsCalendar()
        {
            tcsProvider = new TcsProvider();
            moexProvider = new MoexProvider();
            consoleWriter = new ConsoleWriter();
        }

        public async Task DisplayCalendar()
        {
            if (LastDayOfCalendar > DateTime.MaxValue)
            {
                consoleWriter.PrintText($"{LastDayOfCalendar} is in the past. Please, set correct LastDayOfCalendar param or don't touch it at all.");
                return;
            }
            var watch = Stopwatch.StartNew();

            var positions = await tcsProvider.GetBondsAsync();
            consoleWriter.PrintText($"Tcs latency: {watch.ElapsedMilliseconds} ms.");
            watch.Restart();

            var bonds = await GetDetailedBondsAsync(positions);
            consoleWriter.PrintText($"MOEX latency: {watch.ElapsedMilliseconds} ms.");

            if (AggregateByAccounts)
            {
                consoleWriter.PrintText($"AggregateByAccounts: true");
                bonds = GroupByAccounts(bonds);
            }

            if (DisplayYearResults)
            {
                consoleWriter.PrintText($"DisplayYearResults: true");
                AddYearResults(bonds);
            }

            consoleWriter.PrintBonds(bonds);
        }

        private async Task<List<BondWithDate>> GetDetailedBondsAsync(Position[] tcsPositions)
        {
            var bondsWithDate = new List<BondWithDate>();
            var detailedBondsTasks = tcsPositions.Select(moexProvider.GetByTickerAsync);
            await Task.WhenAll(detailedBondsTasks);

            foreach (var bondTask in detailedBondsTasks)
            {
                var (date, value, currency, freq, bond) = await bondTask;
                var daysInterval = (int)(366 / freq); // I'm not sure about this, but it is correlated with TCS's coupon calendar

                var lastDate = date;
                while(lastDate <= LastDayOfCalendar)
                {
                    bondsWithDate.Add(new BondWithDate()
                    {
                        Currency = currency,
                        Value = value,
                        Date = lastDate,
                        Ticker = bond.Ticker,
                        Name = bond.Name,
                        Lots = bond.Lots
                    });

                    lastDate = lastDate.AddDays(daysInterval);
                } 
            }

            return bondsWithDate;
        }

        private void AddYearResults(List<BondWithDate> bondsWithDate)
        {
            var eachYear = bondsWithDate.GroupBy(x => x.Date.Year).Select(x => new BondWithDate()
            {
                Name = $"Total for {x.Key} year",
                Value = x.Sum(s => s.Value * s.Lots),
                Date = (new DateTime(x.Key + 1, 1, 1).AddSeconds(-1))
            }).ToArray();

            bondsWithDate.AddRange(eachYear);
        }

        private List<BondWithDate> GroupByAccounts(IEnumerable<BondWithDate> bondsWithDate)
        {
            return bondsWithDate.GroupBy(x => new { x.Date, x.Ticker }).Select(x => new BondWithDate()
            {
                Currency = x.First().Currency,
                Value = x.First().Value,
                Date = x.Key.Date,
                Ticker = x.First().Ticker,
                Name = x.First().Name,
                Lots = x.Sum(y => y.Lots)
            }).ToList();
        }
    }
}
