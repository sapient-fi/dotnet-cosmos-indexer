using System.Text.Json.Serialization;

namespace TerraDotnet.TerraFcd.Messages.Wasm;

public record WasmExecuteMessage
{
    [JsonPropertyName("send")] 
    public WasmExecuteMsgSend? Send { get; set; }
        
    [JsonPropertyName("withdraw_voting_tokens")]
    public WasmExecuteMsgWithdrawVotingTokens? WithdrawVotingTokens { get; set; }
        
    [JsonPropertyName("deposit")] 
    public WasmExecuteMsgDeposit? Deposit { get; set; }
        
    [JsonPropertyName("deposit_stable")]
    public WasmExecuteMsgDepositStable? DepositStable { get; set; }

    [JsonPropertyName("swap")] public WasmExecuteMsgSwap? Swap { get; set; } = null;

    [JsonPropertyName("unbond")]
    public WasmExecuteMsgUnbond? Unbond { get; set; }
        
    [JsonPropertyName("claim")]
    public WasmExecuteMsgClaim? Claim { get; set; }

    [JsonPropertyName("withdraw")]
    public WasmExecuteMsgWithdraw? Withdraw { get; set; }
        
    [JsonPropertyName("auto_stake")] 
    public WasmExecuteMsgProvideLiquidity? AutoStake { get; set; }
        
    [JsonPropertyName("provide_liquidity")] 
    public WasmExecuteMsgProvideLiquidity? ProvideLiquidity { get; set; }
        
    [JsonPropertyName("cast_vote")] 
    public WasmExecuteMsgCastVote? CastVote { get; set; }
        
    [JsonPropertyName("mint")]
    public WasmExecuteMsgMint? Mint { get; set; }

    [JsonPropertyName("compound")]
    public WasmExecuteMsgCompound? Compound { get; set; }

    [JsonPropertyName("sweep")]
    public WasmExecuteMsgSweep? Sweep { get; set; }

    [JsonPropertyName("earn")]
    public WasmExecuteMsgEarn? Earn { get; set; }
        
    [JsonPropertyName("update_config")]
    public WasmExecuteMsgUpdateConfig? UpdateConfig { get; set; }

    [JsonPropertyName("transfer")]
    public WasmExecuteMsgTransfer? Transfer { get; set; }

    [JsonPropertyName("end_poll")]
    public WasmExecuteMsgEndPoll? EndPoll { get; set; }
        
    [JsonPropertyName("staking")]
    public WasmExecuteMsgStaking? Staking { get; set; }

    [JsonPropertyName("airdrop")]
    public WasmExecuteMsgAirdrop? Airdrop { get; set; }

    [JsonPropertyName("execute_swap_operations")]
    public WasmExecuteMsgExecuteSwapOperations? ExecuteSwapOperations { get; set; }
}