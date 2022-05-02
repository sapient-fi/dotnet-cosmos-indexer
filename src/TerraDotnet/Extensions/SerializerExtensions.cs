using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Serilog;
using ServiceStack;
using TerraDotnet.TerraFcd.Messages;
using TerraDotnet.TerraFcd.Messages.Bank;
using TerraDotnet.TerraFcd.Messages.Distributions;
using TerraDotnet.TerraFcd.Messages.Gov;
using TerraDotnet.TerraFcd.Messages.Market;
using TerraDotnet.TerraFcd.Messages.Staking;
using TerraDotnet.TerraFcd.Messages.Wasm;

namespace TerraDotnet.Extensions;

public static class SerializerExtensions
{
    private static readonly JsonSerializerOptions Options  = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new JsonNumberToStringConverter()
        }
    };

    
    public static T ToObject<T>(this JsonElement element)
    {
        var json = element.GetRawText();
        if (json.IsBase64String())
        {
            json = Encoding.UTF8.GetString(Convert.FromBase64String(json));
        }
            
        return JsonSerializer.Deserialize<T>(json, Options);
    }

    public static T ToObject<T>(this JsonDocument document)
    {
        var json = document.RootElement.GetRawText();
        return JsonSerializer.Deserialize<T>(json, Options);
    }

    public static T ToObject<T>(this Dictionary<string, JsonElement> dict, string key)
    {
        if (dict == null)
        {
            return default;
        }

        if (!dict.TryGetValue(key, out var val))
        {
            return default;
        }

        return val.ToObject<T>();
    }

    public static T? ToObject<T>(this string json)
    {
        if (json.IsBase64String())
        {
            return ToObjectFromBase64<T>(json);
        }
        
        return JsonSerializer.Deserialize<T>(json, Options);
    }

    public static T? ToObjectFromBase64<T>(this string base64StringMessage)
    {
        return JsonSerializer.Deserialize<T>(Convert.FromBase64String(base64StringMessage), Options);
    }
    
    public static List<IMsg> FromCoreStdTxMessage(this Dictionary<string, JsonElement> dict,
        string key,
        string chainId, 
        string txTransactionHash
    )
    {
        var logger = Log.ForContext(typeof(SerializerExtensions));
        if (dict == null)
        {
            return default;
        }

        if (!dict.TryGetValue(key, out var val))
        {
            return default;
        }

        var intermediate = val.ToObject<List<TxMsg>>();

        var toRet = new List<IMsg>();
        foreach (var inter in intermediate)
        {
            var fromCoreStdTxMessage = inter.FromCoreStdTxMessage(chainId);
            if (fromCoreStdTxMessage == default)
            {
                logger.Error("Unable to parse type {Type} for tx-hash {TxHash}", inter.Type, txTransactionHash);   
                continue;
            }
            toRet.Add(fromCoreStdTxMessage);
        }

        return toRet;
    }

    public static IMsg? FromCoreStdTxMessage(this TxMsg msg, string chainId)
    {
        return msg.Type switch
        {
            "wasm/MsgExecuteContract" => new WasmMsgExecuteContract
            {
                Type = msg.Type,
                Value = chainId.EqualsIgnoreCase("columbus-4")
                    ? msg.Value.ToObject<WasmMsgExecuteContractValueCol4>() : msg.Value.ToObject<WasmMsgExecuteContractValueCol5>()
            },
            "wasm/MsgInstantiateContract" => new WasmMsgInstantiateContract
            {
                Type = msg.Type,
            },
            // TODO complete this type handler
            "bank/MsgSend" => new BankMsgSend
            {
                Type = msg.Type
            },
            // TODO complete this type handler
            "staking/MsgUndelegate" => new StakingMsgUndelegate
            {
                Type = msg.Type
            },
            // TODO complete this type handler
            "staking/MsgBeginRedelegate" => new StakingMsgBeginRedelegate
            {
                Type = msg.Type,
            },
            // TODO complete this type handler
            "staking/MsgDelegate" => new StakingMsgDelegate
            {
                Type = msg.Type,
            },
            // TODO complete this type handler
            "gov/MsgVote" => new GovMsgVote
            {
                Type = msg.Type,
            },
            "distribution/MsgWithdrawDelegationReward" => new DistributionMsgWithdrawDelegationReward
            {
                Type = msg.Type,
                Value = msg.Value.ToObject<DistributionWithdrawDelegationRewardValue>()
            },
            // TODO complete this type handler
            "market/MsgSwap" => new MarketMsgSwap
            {
                Type = msg.Type
            },
            "wasm/MsgMigrateContract" => new WasmMsgMigrateContract
            {
                Type = msg.Type
            },
            _ => null,
        };
    }
        
    public static bool IsBase64String(this string s)
    {
        s = s.Trim();
        return (s.Length % 4 == 0) && Regex.IsMatch(s, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None);

    }
}