using System.Text.Json;
using TerraDotnet.Extensions;
using TerraDotnet.TerraFcd.Messages;
using TerraDotnet.TerraLcd.Messages;

namespace TerraDotnet;

public class TerraMessageParser
{
    public static bool TryParse(Dictionary<string, JsonElement> txMessage, out IMsg? message)
    {
        message = null;
        var parseStatus = false;

        var containsTypeProperty = txMessage.TryGetValue("@type", out var typeProperty);
        if (!containsTypeProperty)
        {
            return parseStatus;
        }

        switch (typeProperty.GetString())
        {
            case "/cosmos.staking.v1beta1.MsgDelegate":
                message = new Terra2DelegateMessage
                {
                    Type = txMessage["@type"].GetString()!,
                    DelegatorAddress = txMessage["delegator_address"].GetString()!,
                    ValidatorAddress = txMessage["validator_address"].GetString()!,
                    Amount = txMessage["amount"].ToObject<CosmosAmount>()
                };
                break;

            case "/cosmos.staking.v1beta1.MsgUndelegate":
                message = new Terra2UndelegateMessage
                {
                    Type = txMessage["@type"].GetString()!,
                    DelegatorAddress = txMessage["delegator_address"].GetString()!,
                    ValidatorAddress = txMessage["validator_address"].GetString()!,
                    Amount = txMessage["amount"].ToObject<CosmosAmount>()
                };
                break;

            case "/cosmos.staking.v1beta1.MsgBeginRedelegate":
                message = new Terra2RedelegateMessage
                {
                    Type = txMessage["@type"].GetString()!,
                    DelegatorAddress = txMessage["delegator_address"].GetString()!,
                    ValidatorSourceAddress = txMessage["validator_src_address"].GetString()!,
                    ValidatorDestinationAddress = txMessage["validator_dst_address"].GetString()!,
                    Amount = txMessage["amount"].ToObject<CosmosAmount>()
                };
                break;

            default:
                // Does not contain a message type we are interested in.
                break;
        }

        if (message is not null)
        {
            parseStatus = true;
        }

        return parseStatus;
    }
}