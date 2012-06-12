using System.Collections.Generic;
using System.Linq;

namespace RavenQuestionnaire.Core.Utility.OrderStrategy
{
    public class MaxMinStrategy
    {
        public IEnumerable<T> Reorder<T>(IEnumerable<T> list)
        {
            return list.OrderByDescending(n => n);
        }
    }
}
