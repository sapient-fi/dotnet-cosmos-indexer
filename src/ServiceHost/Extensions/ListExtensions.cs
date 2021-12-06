using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Invacoil.Kernel;
using ServiceStack;

namespace Invacoil.ServiceRole.TerraMoney.Extensions
{
    public static class ListExtensions
    {

        public static void EnsureUstIsLast(this List<Amount> list)
        {
            if (list.Count != 2)
            {
                return;
            }

            if (list.All(i => !i.Denominator.EqualsIgnoreCase(TerraDenominators.Ust))) return;
            
            if (list[1].Denominator != TerraDenominators.Ust)
            {
                list.Reverse();
            }
        }
    }
}