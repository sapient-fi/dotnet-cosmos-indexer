using System;

namespace TerraDotnet.Extensions;

public static class StringExtensions
{
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
        return denom.Length >= 4 && denom.StartsWith("u", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines whether the given denominator is a terra address reference
    /// </summary>
    /// <param name="denom"></param>
    /// <returns></returns>
    public static bool IsTerraDenominatorAddress(this string denom)
    {
        return denom.StartsWith("terra", StringComparison.OrdinalIgnoreCase);
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