using System;
using System.Collections.Generic;

namespace Invacoil.ServiceRole.Oracle.ExchangeRates.Fiat.LowLevel
{
    public class OpenExchangeRateResponse
    {
        public long Timestamp { get; set; }

        public string Base { get; set; }

        public IDictionary<string, decimal> Rates { get; set; }
    }
}