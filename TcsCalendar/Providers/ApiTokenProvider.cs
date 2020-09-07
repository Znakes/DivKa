using System;

namespace TcsCalendar.Providers
{
    public class ApiTokenProvider
    {
        public string GetToken() => Environment.GetEnvironmentVariable("TCS_TOKEN");
    }
}
