namespace Pylonboard.ServiceHost.TerraDataFetchers;

public static class TerraStakingContracts
{
    public const string MINE_STAKING_CONTRACT = "terra1xu8utj38xuw6mjwck4n97enmavlv852zkcvhgp";
    public const string MINE_BUYBACK_CONTRACT = "terra1dwyfj2m3e3lcyzu6haznqnj57pl769n6947vgv";
    public const string TWD_STAKING_CONTRACT = "terra1l709gpyzpwukpq3g55j7n8kqyzataxlg4swg96";
        
    public const string SPEC_PYLON_FARM = "terra1r3675psl7s2fe0sfh0vut5z4hrywgyyfdrzg95";
}
public static class TerraPylonGatewayContracts
{
    public const string WHITE_WHALE_1 = "terra1srw8lgcp4uqyar22ldeje5p0nx35q2jd93dc3k";
    public const string WHITE_WHALE_2 = "terra1ezxapxsduvp7v3njpcvtadwgwne3ch0muhce6u";
    public const string WHITE_WHALE_3 = "terra1wzer7q9zsug8jxgwp7l6dzd7ehc37nwg9fadef";
        
    public const string LOOP_1 = "terra1p9ns8r3unhvvp6ka3r52h79d0t6wjthxu4dfrs";
    public const string LOOP_2 = "terra1527ks2mulus98lwx7qhdmrv4hug9pgx0m3c95s";
    public const string LOOP_3 = "terra1jtd00mrwdpa6aecvw60mhzrqup054q054u53ch";
        
    public const string ORION = "terra1kmvvzp0vadlr4fvug6zve7736ufnt2x7rvdufm";
        
    public const string VKR_1 = "terra1p625agkeu4vrr4fqnl5c82myhy3z95t6tqycku";
    public const string VKR_2 = "terra1ftl4pt3l3ccjgk4ucndsff73uum888a2kcy779";
    public const string VKR_3 = "terra1yznc2p9q2smx8ku8m20hhv8amdmcj0zcvjh6km";
        
    public const string TERRA_WORLD_1 = "terra1qz6kp8nu5cqy6g679epd2f436p8uyry0aevrxc";
    public const string TERRA_WORLD_2 = "terra14v4g46j8ah9lpwwrnhxh6kyqvytmwd3ma9qvtu";
    public const string TERRA_WORLD_3 = "terra1fyduwdy0ncz8qur0rzp2t7skt4sc3e20w0d7qx";
        
    public const string MINE_1 = "terra19vnwdqz4um0z8f69pc8y0z4ncrcxm4cjf3gevz";
    public const string MINE_2 = "terra1t3wtg074jjscqc5k2hn6l4lsremccm25tt77zp";
    public const string MINE_3 = "terra1za627n8zc8wqg06n9h7khpmjcnlkdkt38rkl3u";
    
    public const string NEXUS = "terra1fmnedmd3732gwyyj47r5p03055mygce98dpte2";
    
    public const string GLOW_1 = "terra1nu4nxjjgw553zhc0k624h7vqmk5z5tj8ufrrzd";
    public const string GLOW_2 = "terra1vwtr0trqz4nuqwy2g2n3szwczp2a4ccsf8hn9j";
    public const string GLOW_3 = "terra1709w9ll57sdmyr8zzqtp423r6cwxyc33hc9xnq";
}

public static class TerraLpFarmingContracts
{
    public const string NEXUS_NLUNA_PSI_FARM = "terra1hs4ev0ghwn4wr888jwm56eztfpau6rjcd8mczc";
        
    public const string NEXUS_PSI_UST_FARM = "terra12kzewegufqprmzl20nhsuwjjq6xu8t8ppzt30a";
        
    public const string PYLON_MINE_UST_FARM = "terra19nek85kaqrvzlxygw20jhy08h3ryjf5kg4ep3l";
        
    public const string STT_INTERSTELLARS_FARM = "terra1v6cagryg27qyk7alp7lq35fttkjyn8cmd73fgv";
        
    public const string STT_INTERSTELLARS_LP_FARM = "terra1snra29afr9efzt6l34wfnhj3jn90hq6rx8jhje";
        
    public const string VALKYRIE_VKR_UST_FARM = "terra1ude6ggsvwrhefw2dqjh4j6r7fdmu9nk6nf2z32";
}

public static class TerraAirdropContracts
{
    public static readonly Dictionary<string, TerraAirdropContractOrigin> Origins = new()
    {
        { "terra146ahqn6d3qgdvmj8cj96hh03dzmeedhsf0kxqm", TerraAirdropContractOrigin.Anchor },
        { "terra1ud39n6c42hmtp2z0qmy8svsk7z3zmdkxzfwcf2", TerraAirdropContractOrigin.Pylon },
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

    Pylon,
        
    Mirror,

    Valkyrie,

    Loop,

    Loopr,
        
    StarTerra,

    Nexus,

    Orion,
        
    Apollo
}