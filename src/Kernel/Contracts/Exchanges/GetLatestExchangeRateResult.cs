using System;

namespace SapientFi.Kernel.Contracts.Exchanges;

public record GetLatestExchangeRateResult
{
    public decimal Value { get; set; }
    public DateTimeOffset ClosedAt { get; set; }
}