using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using static Tinkoff.Trading.OpenApi.Models.Portfolio;

namespace TcsCalendar.Providers
{

    public class MoexProvider 
    {
        // screw it, don't dispose
        private HttpClient client;

        const string MoexUrl = "https://iss.moex.com/iss/securities/{0}";
        
        const string valueXPath = "/document/data/rows/row[@name='COUPONVALUE']/@value";
        const string dateXPath = "/document/data/rows/row[@name='COUPONDATE']/@value";
        const string currencyXPath = "/document/data/rows/row[@name='FACEUNIT']/@value";
        const string frequencyXPath = "/document/data/rows/row[@name='COUPONFREQUENCY']/@value";
        const string rowsXPath = "/document/data/rows/row";

        public MoexProvider()
        {
            client = new HttpClient();
        }

        public async Task<(DateTime date,double value, string currency, double frequency, Position position)> GetByTickerAsync(Position position)
        {
            var resp = await client.GetAsync(string.Format(MoexUrl, position.Ticker));
            resp.EnsureSuccessStatusCode();

            var doc = new XmlDocument();
            doc.LoadXml(await resp.Content.ReadAsStringAsync());

            // if found in MOEX
            if (doc.SelectNodes(rowsXPath).Count > 0)
            {
                var date = DateTime.Parse(doc.SelectSingleNode(dateXPath).Value);
                var value = double.Parse(doc.SelectSingleNode(valueXPath).Value);
                var currency = doc.SelectSingleNode(currencyXPath).Value;
                var freq = double.Parse(doc.SelectSingleNode(frequencyXPath).Value);

                return (date, value, currency, freq, position);
            }
            
            return (DateTime.MinValue, 0, "N/A", 1, position);
        }
    }
}
