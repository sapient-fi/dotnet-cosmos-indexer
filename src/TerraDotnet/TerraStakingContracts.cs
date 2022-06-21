// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo

namespace TerraDotnet;

// TODO update address

public static class TerraStakingContracts
{
    public const string MINE_STAKING_CONTRACT = "terra1xu8utj38xuw6mjwck4n97enmavlv852zkcvhgp";
    public const string MINE_BUYBACK_CONTRACT = "terra1dwyfj2m3e3lcyzu6haznqnj57pl769n6947vgv";
    public const string MINE_TREASURY_CONTRACT = "terra1s354cy63ygs0ydzwxwj0v3x0k8mxzmasgwkqg3";
    public const string TWD_STAKING_CONTRACT = "terra1l709gpyzpwukpq3g55j7n8kqyzataxlg4swg96";
}


public static class TerraswapLpFarmingContracts
{
    public const string NEXUS_NLUNA_PSI_FARM = "terra1hs4ev0ghwn4wr888jwm56eztfpau6rjcd8mczc";
        
    public const string NEXUS_PSI_UST_FARM = "terra12kzewegufqprmzl20nhsuwjjq6xu8t8ppzt30a";
        
    public const string NEXUS_NETH_PSI_FARM = "terra1lws09x0slx892ux526d6atwwgdxnjg58uan8ph";

    public const string STT_INTERSTELLARS_FARM = "terra1v6cagryg27qyk7alp7lq35fttkjyn8cmd73fgv";
        
    public const string STT_INTERSTELLARS_LP_FARM = "terra1snra29afr9efzt6l34wfnhj3jn90hq6rx8jhje";
        
    public const string VALKYRIE_VKR_UST_FARM = "terra1ude6ggsvwrhefw2dqjh4j6r7fdmu9nk6nf2z32";
    
}


public static class TerraAirdropContracts
{
    public static readonly Dictionary<string, TerraAirdropContractOrigin> Origins = new()
    {
        { "terra146ahqn6d3qgdvmj8cj96hh03dzmeedhsf0kxqm", TerraAirdropContractOrigin.Anchor },
        { "terra1kalp2knjm4cs3f59ukr4hdhuuncp648eqrgshw", TerraAirdropContractOrigin.Mirror },
        { "terra1s5ww3afj9ym9k5ceu5m3xmea0t9tl7fmh7r40h", TerraAirdropContractOrigin.Valkyrie },
        { "terra1992lljnteewpz0g398geufylawcmmvgh8l8v96", TerraAirdropContractOrigin.Nexus },
        // LOOP genesis drop
        { "terra1atch4d5t25csx7ranccl48udq94k57js6yh0vk", TerraAirdropContractOrigin.Loop },
        // LOOP LP farming extra rewards
        { "terra1ax2xsp6wegcmlhu3lrng0yhaeknergjq3j2azl", TerraAirdropContractOrigin.Loop },
        // LOOPR airdrop for holding LOOPR
        {"terra1twtgca3p9ynjw30tuqrf3fh270f38fxjfe950c", TerraAirdropContractOrigin.Loopr},
        // LOOP airdrop for holding LOOPR
        {"terra1acwzpr5hmceckk6dumrtrclzzkaz46tasutljl", TerraAirdropContractOrigin.Loop},
        {"terra1rqlu6w83tzrmm04ld97muwakuegn4c09vwgk2l", TerraAirdropContractOrigin.Orion},
        {"terra1sn807fkaddr6y6asl855rlrywygywlhs88r8ps", TerraAirdropContractOrigin.Apollo},
    };
}

public enum TerraAirdropContractOrigin
{
    Anchor,
        
    Mirror,

    Valkyrie,

    Loop,

    Loopr,
        
    StarTerra,

    Nexus,

    Orion,
        
    Apollo
}