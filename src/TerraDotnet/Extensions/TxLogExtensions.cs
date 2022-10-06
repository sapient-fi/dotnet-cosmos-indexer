using TerraDotnet.TerraFcd.Messages;

namespace TerraDotnet.Extensions;

public static class TxLogExtensions
{
    public static TxLogEventAttribute? QueryTxLogsForAttributeFirstOrDefault(
        this List<TxLog> logs,
        string txLogEventPropToFind,
        string attributeKeyToFind
    )
    {
        return logs.QueryTxLogsForAttributeFirstOrDefault(
            txLogEventPropToFind,
            attribute => attribute.Key == attributeKeyToFind);
    }

    public static TxLogEventAttribute? QueryTxLogsForAttributeFirstOrDefault(
        this List<TxLog> logs,
        string txLogEventPropToFind,
        Func<TxLogEventAttribute, bool> predicate
    )
    {
        return logs
            .SelectMany(l =>
                l.Events)
            .Where(e => e.Type.Equals(txLogEventPropToFind, StringComparison.OrdinalIgnoreCase))
            .SelectMany(evt => evt.Attributes)
            .FirstOrDefault(
                predicate
            );
    }
    
    public static TxLogEventAttribute? QueryTxLogsForAttributeLastOrDefault(
        this List<TxLog> logs,
        string txLogEventPropToFind,
        string attributeKeyToFind
    )
    {
        return logs.QueryTxLogsForAttributeLastOrDefault(
            txLogEventPropToFind,
            attribute => attribute.Key == attributeKeyToFind);
    }
    
    public static TxLogEventAttribute? QueryTxLogsForAttributeLastOrDefault(
        this List<TxLog> logs,
        string txLogEventPropToFind,
        Func<TxLogEventAttribute, bool> predicate
    )
    {
        return logs
            .SelectMany(l =>
                l.Events)
            .Where(e => e.Type.Equals(txLogEventPropToFind, StringComparison.OrdinalIgnoreCase))
            .SelectMany(evt => evt.Attributes)
            .LastOrDefault(
                predicate
            );
    }
    public static IEnumerable<TxLogEventAttribute> QueryTxLogsForAttributes(
        this List<TxLog> logs,
        string txLogEventPropToFind,
        Func<TxLogEventAttribute, bool> predicate
    )
    {
        return logs
            .SelectMany(l =>
                l.Events)
            .Where(e => e.Type.Equals(txLogEventPropToFind, StringComparison.OrdinalIgnoreCase))
            .SelectMany(evt => evt.Attributes)
            .Where(
                predicate
            );
    }
}