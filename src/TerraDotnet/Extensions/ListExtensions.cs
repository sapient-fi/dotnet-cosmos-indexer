namespace TerraDotnet.Extensions;

public static class ListExtensions
{

    public static void EnsureUstIsLast(this List<TerraAmount> list)
    {
        if (list.Count != 2)
        {
            return;
        }

        if (list.All(i => !i.Denominator.Equals(TerraDenominators.Ust, StringComparison.CurrentCultureIgnoreCase))) return;
            
        if (list[1].Denominator != TerraDenominators.Ust)
        {
            list.Reverse();
        }
    }
}