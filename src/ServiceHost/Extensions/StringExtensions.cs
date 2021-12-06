using System.Text.RegularExpressions;
using Pylonboard.Kernel;
using Pylonboard.ServiceHost.Oracles.TerraFcd.Messages;
using ServiceStack;

namespace Pylonboard.ServiceHost.Extensions;

public static class StringExtensions
{
    public static TerraStringAmount ToTerraStringAmount(this string str)
    {
        var amountDenomRegex = new Regex("^([0-9]+)([a-z0-9]+)$",
            RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant);

        var match = amountDenomRegex.Match(str.Trim());
            
        if(match.Groups.Count != 3)
        {
            throw new ArgumentException($"Input amount seems invalid: {str}", nameof(str));
        }

        return new TerraStringAmount
        {
            Amount = match.Groups[1].Value,
            Denominator = match.Groups[2].Value,
        };
    }

    public static TerraAmount ToTerraAmount(this string str)
    {
        var terraStrAmount = str.ToTerraStringAmount();

        var amount = new TerraAmount
        {
            Denominator = terraStrAmount.Denominator,
            Value = terraStrAmount.Amount.ToInt64() / 1_000_000m,
        };

        // is this a terra address we need to convert?
        if (amount.Denominator.StartsWithIgnoreCase("terra"))
        {
            amount.Denominator = TerraDenominators.AssetTokenAddressToDenominator[amount.Denominator];
        }
        else if (amount.Denominator.IsMuDemominator())
        {
            amount.Denominator = amount.Denominator.FromNativeDenomToDenom();
        }

        return amount;
    }

    /// <summary>
    /// Determines whether a given currency denominator (like uluna) is a U amount
    /// </summary>
    /// <remarks>
    /// This is solely a herustic based off the fact that the length of the denominator is greater than 3
    /// and whether it starts with `u`
    /// </remarks>
    /// <param name="denom">The denominator to test</param>
    /// <returns></returns>
    public static bool IsMuDemominator(this string denom)
    {
        return denom.Length >= 4 && denom.StartsWithIgnoreCase("u");
    }

    /// <summary>
    /// Determines whether the given denominator is a terra address reference
    /// </summary>
    /// <param name="denom"></param>
    /// <returns></returns>
    public static bool IsTerraDenominatorAddress(this string denom)
    {
        return denom.StartsWithIgnoreCase("terra");
    }

    public static string FromNativeDenomToDenom(this string denom)
    {
        return denom switch
        {
            "uusd" => TerraDenominators.Ust,
            _ => throw new ArgumentOutOfRangeException(nameof(denom), $"No handler for native denom : {denom}"),
        };
    }
}