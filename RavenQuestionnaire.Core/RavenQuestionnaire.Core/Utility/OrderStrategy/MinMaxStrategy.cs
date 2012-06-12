using System.Linq;
using System.Collections.Generic;

namespace RavenQuestionnaire.Core.Utility.OrderStrategy
{
    public class MinMaxStrategy:IOrderStrategy
    {
        public IEnumerable<T> Reorder<T>(IEnumerable<T> list)
        {
            return list/*.OrderBy(n=>n)*/;
        }
    }
}