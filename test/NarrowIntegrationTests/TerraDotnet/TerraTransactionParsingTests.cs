using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using SapientFi.Infrastructure.Terra2.Storage;
using TerraDotnet;
using TerraDotnet.TerraLcd.Messages;
using Xunit;

namespace NarrowIntegrationTests.TerraDotnet;

public class TerraTransactionParsingTests : IntegrationBaseTest
{
    private readonly Terra2RawRepository _rawRepository;

    public TerraTransactionParsingTests()
    {
        _rawRepository = new Terra2RawRepository(GetDbConnectionFactory());
    }
    
    private GetTransactionsMatchingQueryResponse? GetParsedJson_01()
    {
        var json = File.ReadAllText("TerraDotnet/GetTransactionsMatchingQueryResponse_01.json");

        return JsonSerializer.Deserialize<GetTransactionsMatchingQueryResponse>(
            json,
            TerraJsonSerializerOptions.GetThem()
        );
    }
    
    private GetTransactionsMatchingQueryResponse? GetParsedJson_02()
    {
        var json = File.ReadAllText("TerraDotnet/GetTransactionsMatchingQueryResponse_02.json");

        return JsonSerializer.Deserialize<GetTransactionsMatchingQueryResponse>(
            json,
            TerraJsonSerializerOptions.GetThem()
        );
    }
    
    [Fact]
    public async void SerializingAndDeserializingTransactionFromDatabaseProducesOriginalTransaction()
    {
        #region ___ARRANGE___
        const int ID = 2;

        await EnsureEmptyAsync<Terra2RawTransactionEntity>();

        var tx = GetParsedJson_02()!.TransactionResponses[0];
        var rawTxEntity = new Terra2RawTransactionEntity
        {
            Id = ID,
            Height = tx.HeightAsInt,
            CreatedAt = tx.CreatedAt,
            TxHash = tx.TransactionHash,
            RawTx = JsonSerializer.Serialize(tx, TerraJsonSerializerOptions.GetThem())
        };

        #endregion

        // ************************************************************************************************

        #region ___ACT___

        await _rawRepository.SaveRawTransactionAsync(rawTxEntity);
        var rawTxEntityFromDb = await _rawRepository.GetByIdOrDefaultAsync(ID);

        var rawTxDes = JsonSerializer.Deserialize<LcdTxResponse>(rawTxEntity.RawTx);
        var txFromDbDes = JsonSerializer.Deserialize<LcdTxResponse>(rawTxEntityFromDb!.RawTx);

        #endregion

        // ************************************************************************************************

        #region ___ASSERT___

        Assert.Equal(2, rawTxDes!.Transaction.Body.Messages.Count);
        Assert.Equal(2, txFromDbDes!.Transaction.Body.Messages.Count);


        Assert.Equal(rawTxEntity.Id, rawTxEntityFromDb.Id);
        Assert.Equal(rawTxEntity.Height, rawTxEntityFromDb.Height);
        Assert.Equal(rawTxEntity.CreatedAt, rawTxEntityFromDb.CreatedAt);
        Assert.Equal(rawTxEntity.TxHash, rawTxEntityFromDb.TxHash);

        // RawTx equality
        var txBeforeDb = rawTxDes;
        var txFromDb = txFromDbDes;

        Assert.Equal(txBeforeDb.Code,            txFromDb.Code);
        Assert.Equal(txBeforeDb.CodeSpace,       txFromDb.CodeSpace);
        Assert.Equal(txBeforeDb.Data,            txFromDb.Data);
        Assert.Equal(txBeforeDb.CreatedAt,       txFromDb.CreatedAt);
        Assert.Equal(txBeforeDb.Events.Count,    txFromDb.Events.Count);
        Assert.Equal(txBeforeDb.GasUsed,         txFromDb.GasUsed);
        Assert.Equal(txBeforeDb.GasWanted,       txFromDb.GasWanted);
        Assert.Equal(txBeforeDb.Height,          txFromDb.Height);
        Assert.Equal(txBeforeDb.HeightAsInt,     txFromDb.HeightAsInt);
        Assert.Equal(txBeforeDb.Info,            txFromDb.Info);
        Assert.Equal(txBeforeDb.Logs.Count,      txFromDb.Logs.Count);
        Assert.Equal(txBeforeDb.RawGasUsed,      txFromDb.RawGasUsed);
        Assert.Equal(txBeforeDb.RawGasWanted,    txFromDb.RawGasWanted);
        Assert.Equal(txBeforeDb.RawLog,          txFromDb.RawLog);
        Assert.Equal(txBeforeDb.TransactionHash, txFromDb.TransactionHash);
        
        #endregion
    }
    
    [Fact]
    public async void CanDeserialize_TxFromDb()
    {
        await EnsureEmptyAsync<Terra2RawTransactionEntity>();
        var actual = GetParsedJson_01();
        
        Assert.NotNull(actual);
        
        // transactions
        Assert.Equal(3, actual!.TransactionResponses.Count);

        var tx = actual.TransactionResponses[0];
        
        var rawTxEntity = new Terra2RawTransactionEntity
        {
            Id = 1,
            Height = tx.HeightAsInt,
            CreatedAt = tx.CreatedAt,
            TxHash = tx.TransactionHash,
            RawTx = JsonSerializer.Serialize(tx, TerraJsonSerializerOptions.GetThem())
        };
        
        var ct = new CancellationToken();
        await _rawRepository.SaveRawTransactionAsync(rawTxEntity, ct);

        var txFromDb = await _rawRepository.GetByIdOrDefaultAsync(1, ct);
        
        Assert.NotNull(txFromDb);
        Assert.Equal(201, txFromDb!.Height);
        Assert.Equal("74B8212F209C7F225F79B7C0CA064160CC3C9D589A4F4AB6849071A0DAFED5A3", txFromDb.TxHash);
        Assert.Equal(new DateTimeOffset(2022, 5, 28, 6, 26, 53, TimeSpan.Zero), txFromDb.CreatedAt);


        var rawTxFromDb = JsonSerializer.Deserialize<LcdTxResponse>(txFromDb.RawTx)!;
        
        Assert.Equal("201", rawTxFromDb.Height);
        Assert.Equal(201, rawTxFromDb.HeightAsInt);
        Assert.Equal("74B8212F209C7F225F79B7C0CA064160CC3C9D589A4F4AB6849071A0DAFED5A3", rawTxFromDb.TransactionHash);
        Assert.Empty(rawTxFromDb.CodeSpace);
        Assert.Equal(0, rawTxFromDb.Code);
        Assert.Equal("0A3C0A2A2F636F736D6F732E7374616B696E672E763162657461312E4D7367426567696E526564656C6567617465120E0A0C08ADE0B595061095B3998502", rawTxFromDb.Data);
        Assert.Equal("[{\"events\":[{\"type\":\"coin_received\",\"attributes\":[{\"key\":\"receiver\",\"value\":\"terra1wcff43z8jweneamztk8uy74w3sy7rlmudxjdc0\"},{\"key\":\"amount\",\"value\":\"22551uluna\"}]},{\"type\":\"coin_spent\",\"attributes\":[{\"key\":\"spender\",\"value\":\"terra1jv65s3grqf6v6jl3dp4t6c9t9rk99cd8pm7utl\"},{\"key\":\"amount\",\"value\":\"22551uluna\"}]},{\"type\":\"message\",\"attributes\":[{\"key\":\"action\",\"value\":\"/cosmos.staking.v1beta1.MsgBeginRedelegate\"},{\"key\":\"sender\",\"value\":\"terra1jv65s3grqf6v6jl3dp4t6c9t9rk99cd8pm7utl\"},{\"key\":\"module\",\"value\":\"staking\"},{\"key\":\"sender\",\"value\":\"terra1wcff43z8jweneamztk8uy74w3sy7rlmudxjdc0\"}]},{\"type\":\"redelegate\",\"attributes\":[{\"key\":\"source_validator\",\"value\":\"terravaloper1ktu7a6wqlk6vlywf4rt6wfcxuphc0es27p0qvx\"},{\"key\":\"destination_validator\",\"value\":\"terravaloper10c04ysz9uznx2mkenuk3j3esjczyqh0j783nzt\"},{\"key\":\"amount\",\"value\":\"1932000000uluna\"},{\"key\":\"completion_time\",\"value\":\"2022-06-18T06:26:53Z\"}]},{\"type\":\"transfer\",\"attributes\":[{\"key\":\"recipient\",\"value\":\"terra1wcff43z8jweneamztk8uy74w3sy7rlmudxjdc0\"},{\"key\":\"sender\",\"value\":\"terra1jv65s3grqf6v6jl3dp4t6c9t9rk99cd8pm7utl\"},{\"key\":\"amount\",\"value\":\"22551uluna\"}]}]}]", rawTxFromDb.RawLog);
        Assert.Single(rawTxFromDb.Logs);

        var log = rawTxFromDb.Logs[0];
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
        
        Assert.Empty(rawTxFromDb.Info);
        Assert.Equal("448586", rawTxFromDb.RawGasWanted);
        Assert.Equal(448586, rawTxFromDb.GasWanted);
        Assert.Equal("261855", rawTxFromDb.RawGasUsed);
        Assert.Equal(261855, rawTxFromDb.GasUsed);
        Assert.NotNull(rawTxFromDb.Transaction);
        Assert.Equal(new DateTimeOffset(2022, 5, 28, 6, 26, 53, TimeSpan.Zero), rawTxFromDb.CreatedAt);
        
        Assert.Equal(14, rawTxFromDb.Events.Count);

        var event1 = rawTxFromDb.Events[0];
        Assert.Equal("coin_spent", event1.Type);
        Assert.Equal(2, event1.Attributes.Count);
        Assert.Equal("c3BlbmRlcg==", event1.Attributes[0].Key);
        Assert.Equal("dGVycmExd2NmZjQzejhqd2VuZWFtenRrOHV5NzR3M3N5N3JsbXVkeGpkYzA=", event1.Attributes[0].Value);
        Assert.True(event1.Attributes[0].Index);
        Assert.Equal("YW1vdW50", event1.Attributes[1].Key);
        Assert.Equal("NjcyODh1bHVuYQ==", event1.Attributes[1].Value);
        Assert.True(event1.Attributes[1].Index);

        
        var messages = rawTxFromDb.Transaction.Body.Messages;
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
    }
}
