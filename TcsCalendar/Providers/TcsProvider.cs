using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tinkoff.Trading.OpenApi.Models;
using Tinkoff.Trading.OpenApi.Network;
using static Tinkoff.Trading.OpenApi.Models.Portfolio;

namespace TcsCalendar.Providers
{
    public class TcsProvider
    {
        string token = Environment.GetEnvironmentVariable("TCS_TOKEN");

        public async Task<Position[]> GetBondsAsync()
        {
            var connection = ConnectionFactory.GetConnection(token);
            var context = connection.Context;

            var accounts = await context.AccountsAsync();

            var positions = new List<Position>();

            foreach (var account in accounts)
            {
                var portfolio = await context.PortfolioAsync(account.BrokerAccountId);
                positions.AddRange(portfolio.Positions);
            }

            return positions.Where(p => p.InstrumentType == InstrumentType.Bond).ToArray();
        }
    }
}
