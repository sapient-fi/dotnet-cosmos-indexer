namespace TerraDotnet;

public static class TerraDenominators
{
    public const string Luna = "LUNA";
    public const string nLuna = "nLuna";
    public const string nEth = "nEth";
    public const string Anc = "ANC";
    public const string Mine = "MINE";
    public const string Mir = "MIR";
    public const string Loop = "LOOP";
    public const string Vkr = "VKR";
    public const string Stt = "STT";
    public const string Psi = "PSI";
    public const string Twd = "TWD";
    public const string Ust = "UST";
    public const string aUst = "aUST";
    public const string Loopr = "LOOPR";
    public const string Orion = "ORION";
    public const string Apollo = "APOLLO";
    public const string bEth = "bEth";
    public const string bPsiDP = "bPsiDP";
    public const string Whale = "WHALE";
    public const string Glow = "GLOW";
    public const string Sayve = "SAYVE";
    public const string Xdefi = "XDEFI";
    public const string BpsiDP_24m = "bPSIDP-24m";
    public const string Kuji = "KUJI";
    private const string Astro = "ASTRO";
    public const string Arts = "ARTS";
    public const string WCoin = "WCOIN";

    public static readonly Dictionary<string, string> AssetTokenAddressToDenominator = new()
    {
        { TerraTokenContracts.PSI, Psi },
        { TerraTokenContracts.NLUNA, nLuna },
        { TerraTokenContracts.LOOP, Loop },
        { TerraTokenContracts.ANC, Anc },
        { TerraTokenContracts.MINE, Mine },
        { TerraTokenContracts.STT, Stt },
        { TerraTokenContracts.VKR, Vkr },
        { TerraTokenContracts.AUST, aUst },
        { TerraTokenContracts.ORION, Orion },
        { TerraTokenContracts.TWD, Twd },
        { TerraTokenContracts.APOLLO, Apollo },
        { TerraTokenContracts.NETH, nEth },
        { TerraTokenContracts.XDEFI, Xdefi }, //whXDEFI - this is what the Pylon contracts are using
        { TerraTokenContracts.USD, Ust},
        { TerraTokenContracts.BPSI_DP_24M, BpsiDP_24m},
        { TerraTokenContracts.ULUNA, Luna},
        { TerraTokenContracts.LUNAX, Luna},
        { TerraTokenContracts.KUJI, Kuji},
        { TerraTokenContracts.ASTRO, Astro},
        { TerraTokenContracts.WHALE, Whale},
        { TerraTokenContracts.ARTS, Arts},
        { TerraTokenContracts.WCOIN, WCoin},
    };


    /// <summary>
    /// Try to get a known denom from the token address.
    /// Returns the contract addr as fallback
    /// </summary>
    /// <param name="contractAddr"></param>
    /// <returns></returns>
    public static string TryGetDenominator(string contractAddr)
    {
        return !AssetTokenAddressToDenominator.TryGetValue(contractAddr, out var denom) ? contractAddr : denom;
    }

    public static decimal GetDenomOrTokenDivisor(string denomOrToken)
    {
        return denomOrToken switch
        {
            TerraTokenContracts.XDEFI => 100_000_000,
            TerraTokenContracts.ORION => 100_000_000,
            // Default is that we use 6 decimal places
            _ => 1_000_000m,
        };
    }
}