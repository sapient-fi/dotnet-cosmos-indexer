namespace SapientFi.Kernel.Extensions;

public static class DateTimeExtensions_cs
{
    public static string ToSql(this DateTime dt)
    {
        return dt.ToString("yyyy-MM-dd HH:mm:ss.fff");
    }
}
