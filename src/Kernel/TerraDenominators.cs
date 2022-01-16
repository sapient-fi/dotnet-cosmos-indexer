namespace Pylonboard.Kernel;

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

    public static readonly Dictionary<string, string> AssetTokenAddressToDenominator = new()
    {
        { "terra12897djskt9rge8dtmm86w654g7kzckkd698608", Psi },
        { "terra10f2mt82kjnkxqj2gepgwl637u2w4ue2z5nhz5j", nLuna },
        { "terra1nef5jf6c7js9x6gkntlehgywvjlpytm7pcgkn4", Loop },
        { "terra14z56l0fp2lsf86zy3hty2z47ezkhnthtr9yq76", Anc },
        { "terra1kcthelkax4j9x8d3ny6sdag0qmxxynl3qtcrpy", Mine },
        { "terra13xujxcrc9dqft4p9a8ls0w3j0xnzm6y2uvve8n", Stt },
        { "terra1dy9kmlm4anr92e42mrkjwzyvfqwz66un00rwr5", Vkr },
        { "terra1hzh9vpxhsk8253se0vv5jj6etdvxu3nv8z07zu", aUst },
        { "terra1mddcdx0ujx89f38gu7zspk2r2ffdl5enyz2u03", Orion },
        { "terra19djkaepjjswucys4npd5ltaxgsntl7jf0xz7w6", Twd },
        { "terra100yeqvww74h4yaejj6h733thgcafdaukjtw397", Apollo },
        { "terra178v546c407pdnx5rer3hu8s2c0fc924k74ymnn", nEth },
    };

}