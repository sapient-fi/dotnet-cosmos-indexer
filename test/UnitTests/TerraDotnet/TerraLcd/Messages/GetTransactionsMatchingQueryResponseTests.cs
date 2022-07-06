using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using SapientFi.Infrastructure.Terra2.Storage;
using TerraDotnet;
using TerraDotnet.TerraLcd.Messages;
using Xunit;

namespace UnitTests.TerraDotnet.TerraLcd.Messages;

public class GetTransactionsMatchingQueryResponseTests
{
    private GetTransactionsMatchingQueryResponse? GetParsedJson()
    {
        var json = File.ReadAllText("TerraDotnet/TerraLcd/Messages/GetTransactionsMatchingQueryResponse_01.json");

        return JsonSerializer.Deserialize<GetTransactionsMatchingQueryResponse>(json,
            TerraJsonSerializerOptions.GetThem()
        );
    }

    /* ************************************************************************************************************
     */
    [Fact]
    public void CanDeserialize_txs()
    {
        var actual = GetParsedJson();

        Assert.NotNull(actual);

        // transactions
        Assert.Equal(3, actual!.Transactions.Count);

        var first = actual.Transactions[0];

        Assert.NotNull(first.Body);
        Assert.Single(first.Body.Messages);
        Assert.Empty(first.Body.Memo);
        Assert.Equal("0", first.Body.TimeoutHeight);
    }


    /* ************************************************************************************************************
     */
    [Fact]
    public void CanDeserialize_tx_responses()
    {
        var actual = GetParsedJson();

        Assert.NotNull(actual);

        // transactions
        Assert.Equal(3, actual!.TransactionResponses.Count);

        var first = actual.TransactionResponses[0];

        Assert.Equal("201", first.Height);
        Assert.Equal(201, first.HeightAsInt);
        Assert.Equal("74B8212F209C7F225F79B7C0CA064160CC3C9D589A4F4AB6849071A0DAFED5A3", first.TransactionHash);
        Assert.Empty(first.CodeSpace);
        Assert.Equal(0, first.Code);
        Assert.Equal(
            "0A3C0A2A2F636F736D6F732E7374616B696E672E763162657461312E4D7367426567696E526564656C6567617465120E0A0C08ADE0B595061095B3998502",
            first.Data
        );
        Assert.Equal(
            "[{\"events\":[{\"type\":\"coin_received\",\"attributes\":[{\"key\":\"receiver\",\"value\":\"terra1wcff43z8jweneamztk8uy74w3sy7rlmudxjdc0\"},{\"key\":\"amount\",\"value\":\"22551uluna\"}]},{\"type\":\"coin_spent\",\"attributes\":[{\"key\":\"spender\",\"value\":\"terra1jv65s3grqf6v6jl3dp4t6c9t9rk99cd8pm7utl\"},{\"key\":\"amount\",\"value\":\"22551uluna\"}]},{\"type\":\"message\",\"attributes\":[{\"key\":\"action\",\"value\":\"/cosmos.staking.v1beta1.MsgBeginRedelegate\"},{\"key\":\"sender\",\"value\":\"terra1jv65s3grqf6v6jl3dp4t6c9t9rk99cd8pm7utl\"},{\"key\":\"module\",\"value\":\"staking\"},{\"key\":\"sender\",\"value\":\"terra1wcff43z8jweneamztk8uy74w3sy7rlmudxjdc0\"}]},{\"type\":\"redelegate\",\"attributes\":[{\"key\":\"source_validator\",\"value\":\"terravaloper1ktu7a6wqlk6vlywf4rt6wfcxuphc0es27p0qvx\"},{\"key\":\"destination_validator\",\"value\":\"terravaloper10c04ysz9uznx2mkenuk3j3esjczyqh0j783nzt\"},{\"key\":\"amount\",\"value\":\"1932000000uluna\"},{\"key\":\"completion_time\",\"value\":\"2022-06-18T06:26:53Z\"}]},{\"type\":\"transfer\",\"attributes\":[{\"key\":\"recipient\",\"value\":\"terra1wcff43z8jweneamztk8uy74w3sy7rlmudxjdc0\"},{\"key\":\"sender\",\"value\":\"terra1jv65s3grqf6v6jl3dp4t6c9t9rk99cd8pm7utl\"},{\"key\":\"amount\",\"value\":\"22551uluna\"}]}]}]",
            first.RawLog
        );
        Assert.Single(first.Logs);

        var log = first.Logs[0];
        Assert.Equal(0, log.MessageIndex);
        Assert.Empty(log.Log);
        Assert.Equal(5, log.Events.Count);

        var logEvent2 = log.Events[1];
        Assert.Equal("coin_spent", logEvent2.Type);
        Assert.Equal(2, logEvent2.Attributes.Count);
        Assert.Equal("spender", logEvent2.Attributes[0].Key);
        Assert.Equal("terra1jv65s3grqf6v6jl3dp4t6c9t9rk99cd8pm7utl", logEvent2.Attributes[0].Value);
        Assert.Equal("amount", logEvent2.Attributes[1].Key);
        Assert.Equal("22551uluna", logEvent2.Attributes[1].Value);

        Assert.Empty(first.Info);
        Assert.Equal("448586", first.RawGasWanted);
        Assert.Equal(448586, first.GasWanted);
        Assert.Equal("261855", first.RawGasUsed);
        Assert.Equal(261855, first.GasUsed);
        Assert.NotNull(first.Transaction);
        Assert.Equal(new DateTimeOffset(2022, 5, 28, 6, 26, 53, TimeSpan.Zero), first.CreatedAt);

        Assert.Equal(14, first.Events.Count);

        var event1 = first.Events[0];
        Assert.Equal("coin_spent", event1.Type);
        Assert.Equal(2, event1.Attributes.Count);
        Assert.Equal("c3BlbmRlcg==", event1.Attributes[0].Key);
        Assert.Equal("dGVycmExd2NmZjQzejhqd2VuZWFtenRrOHV5NzR3M3N5N3JsbXVkeGpkYzA=", event1.Attributes[0].Value);
        Assert.True(event1.Attributes[0].Index);
        Assert.Equal("YW1vdW50", event1.Attributes[1].Key);
        Assert.Equal("NjcyODh1bHVuYQ==", event1.Attributes[1].Value);
        Assert.True(event1.Attributes[1].Index);



        var messages = first.Transaction.Body.Messages;
        Assert.Single(messages);

        var parseStatusResult = TerraMessageParser.TryParse(messages[0], out var parsedMessage);
        Assert.True(parseStatusResult);
        Assert.NotNull(parsedMessage);

        Assert.Equal("/cosmos.staking.v1beta1.MsgBeginRedelegate", parsedMessage!.Type);
        var parsedRedelegateMessage = (Terra2RedelegateMessage)parsedMessage;

        Assert.Equal("terra1wcff43z8jweneamztk8uy74w3sy7rlmudxjdc0", parsedRedelegateMessage.DelegatorAddress);
        Assert.Equal("terravaloper1ktu7a6wqlk6vlywf4rt6wfcxuphc0es27p0qvx",
            parsedRedelegateMessage.ValidatorSourceAddress
        );
        Assert.Equal("terravaloper10c04ysz9uznx2mkenuk3j3esjczyqh0j783nzt",
            parsedRedelegateMessage.ValidatorDestinationAddress
        );

        Assert.NotNull(parsedRedelegateMessage.Amount);
        Assert.Equal("uluna", parsedRedelegateMessage.Amount!.Denominator);
        Assert.Equal("1932000000", parsedRedelegateMessage.Amount!.Amount);
        //*/
    }
    
    [Fact]
    public void CanDeserialize_LcdTxResponse()
    {
        const int ID = 1;
        var actual = GetParsedJson();
        
        // transactions
        Assert.Equal(3, actual!.TransactionResponses.Count);

        var tx = actual.TransactionResponses[0];

        var rawTxEntity = new Terra2RawTransactionEntity
        {
            Id = ID,
            Height = tx.HeightAsInt,
            CreatedAt = tx.CreatedAt,
            TxHash = tx.TransactionHash,
            RawTx = JsonSerializer.Serialize(tx, TerraJsonSerializerOptions.GetThem())
        };

        #region rawTransaction
        var rawTransaction = rawTxEntity.RawTx;

        var deserializedTx = JsonSerializer.Deserialize<LcdTxResponse>(rawTransaction)!;
        
        Assert.Equal("201", deserializedTx.Height);
        Assert.Equal(201, deserializedTx.HeightAsInt);
        Assert.Equal("74B8212F209C7F225F79B7C0CA064160CC3C9D589A4F4AB6849071A0DAFED5A3", deserializedTx.TransactionHash);
        Assert.Empty(deserializedTx.CodeSpace);
        Assert.Equal(0, deserializedTx.Code);
        Assert.Equal("0A3C0A2A2F636F736D6F732E7374616B696E672E763162657461312E4D7367426567696E526564656C6567617465120E0A0C08ADE0B595061095B3998502", deserializedTx.Data);
        Assert.Equal("[{\"events\":[{\"type\":\"coin_received\",\"attributes\":[{\"key\":\"receiver\",\"value\":\"terra1wcff43z8jweneamztk8uy74w3sy7rlmudxjdc0\"},{\"key\":\"amount\",\"value\":\"22551uluna\"}]},{\"type\":\"coin_spent\",\"attributes\":[{\"key\":\"spender\",\"value\":\"terra1jv65s3grqf6v6jl3dp4t6c9t9rk99cd8pm7utl\"},{\"key\":\"amount\",\"value\":\"22551uluna\"}]},{\"type\":\"message\",\"attributes\":[{\"key\":\"action\",\"value\":\"/cosmos.staking.v1beta1.MsgBeginRedelegate\"},{\"key\":\"sender\",\"value\":\"terra1jv65s3grqf6v6jl3dp4t6c9t9rk99cd8pm7utl\"},{\"key\":\"module\",\"value\":\"staking\"},{\"key\":\"sender\",\"value\":\"terra1wcff43z8jweneamztk8uy74w3sy7rlmudxjdc0\"}]},{\"type\":\"redelegate\",\"attributes\":[{\"key\":\"source_validator\",\"value\":\"terravaloper1ktu7a6wqlk6vlywf4rt6wfcxuphc0es27p0qvx\"},{\"key\":\"destination_validator\",\"value\":\"terravaloper10c04ysz9uznx2mkenuk3j3esjczyqh0j783nzt\"},{\"key\":\"amount\",\"value\":\"1932000000uluna\"},{\"key\":\"completion_time\",\"value\":\"2022-06-18T06:26:53Z\"}]},{\"type\":\"transfer\",\"attributes\":[{\"key\":\"recipient\",\"value\":\"terra1wcff43z8jweneamztk8uy74w3sy7rlmudxjdc0\"},{\"key\":\"sender\",\"value\":\"terra1jv65s3grqf6v6jl3dp4t6c9t9rk99cd8pm7utl\"},{\"key\":\"amount\",\"value\":\"22551uluna\"}]}]}]", deserializedTx.RawLog);
        Assert.Single(deserializedTx.Logs);

        var log = deserializedTx.Logs[0];
        Assert.Equal(0, log.MessageIndex);
        Assert.Empty(log.Log);
        Assert.Equal(5, log.Events.Count);
        
        var logEvent2 = log.Events[1];
        Assert.Equal("coin_spent", logEvent2.Type);
        Assert.Equal(2, logEvent2.Attributes.Count);
        Assert.Equal("spender", logEvent2.Attributes[0].Key);
        Assert.Equal("terra1jv65s3grqf6v6jl3dp4t6c9t9rk99cd8pm7utl", logEvent2.Attributes[0].Value);
        Assert.Equal("amount", logEvent2.Attributes[1].Key);
        Assert.Equal("22551uluna", logEvent2.Attributes[1].Value);
        
        Assert.Empty(deserializedTx.Info);
        Assert.Equal("448586", deserializedTx.RawGasWanted);
        Assert.Equal(448586, deserializedTx.GasWanted);
        Assert.Equal("261855", deserializedTx.RawGasUsed);
        Assert.Equal(261855, deserializedTx.GasUsed);
        Assert.NotNull(deserializedTx.Transaction);
        Assert.Equal(new DateTimeOffset(2022, 5, 28, 6, 26, 53, TimeSpan.Zero), deserializedTx.CreatedAt);
        
        Assert.Equal(14, deserializedTx.Events.Count);

        var event1 = deserializedTx.Events[0];
        Assert.Equal("coin_spent", event1.Type);
        Assert.Equal(2, event1.Attributes.Count);
        Assert.Equal("c3BlbmRlcg==", event1.Attributes[0].Key);
        Assert.Equal("dGVycmExd2NmZjQzejhqd2VuZWFtenRrOHV5NzR3M3N5N3JsbXVkeGpkYzA=", event1.Attributes[0].Value);
        Assert.True(event1.Attributes[0].Index);
        Assert.Equal("YW1vdW50", event1.Attributes[1].Key);
        Assert.Equal("NjcyODh1bHVuYQ==", event1.Attributes[1].Value);
        Assert.True(event1.Attributes[1].Index);

        List<Dictionary<string, JsonElement>> messages = deserializedTx.Transaction.Body.Messages;
        Assert.Single(messages);

        var parseStatusResult = TerraMessageParser.TryParse(messages[0], out var parsedMessage);
        Assert.True(parseStatusResult);
        Assert.NotNull(parsedMessage);
        
        Assert.Equal("/cosmos.staking.v1beta1.MsgBeginRedelegate", parsedMessage!.Type);
        var parsedRedelegateMessage = (Terra2RedelegateMessage)parsedMessage;
        
        Assert.Equal("terra1wcff43z8jweneamztk8uy74w3sy7rlmudxjdc0", parsedRedelegateMessage.DelegatorAddress);
        Assert.Equal("terravaloper1ktu7a6wqlk6vlywf4rt6wfcxuphc0es27p0qvx", parsedRedelegateMessage.ValidatorSourceAddress);
        Assert.Equal("terravaloper10c04ysz9uznx2mkenuk3j3esjczyqh0j783nzt", parsedRedelegateMessage.ValidatorDestinationAddress);
        
        Assert.NotNull(parsedRedelegateMessage.Amount);
        Assert.Equal("uluna", parsedRedelegateMessage.Amount!.Denominator);
        Assert.Equal("1932000000", parsedRedelegateMessage.Amount!.Amount);
        //*/
        #endregion
        
        /* **************************************************************** */

        #region rawTxEntity
        
        Assert.Equal(201, rawTxEntity.Height);
        Assert.Equal("74B8212F209C7F225F79B7C0CA064160CC3C9D589A4F4AB6849071A0DAFED5A3", rawTxEntity.TxHash);

        Assert.Equal(new DateTimeOffset(2022, 5, 28, 6, 26, 53, TimeSpan.Zero), rawTxEntity.CreatedAt);
        
        #endregion
    }

    /* ************************************************************************************************************
     */
    [Fact]
    public void CanDeserialize_pagination()
    {
        var actual = GetParsedJson();
        
        Assert.NotNull(actual);
        
        Assert.NotNull(actual!.Pagination);
        
        Assert.Null(actual.Pagination.NextKey);
        Assert.Equal("540", actual.Pagination.Total);
        Assert.Equal(540, actual.Pagination.TotalAsInt);
    }
}