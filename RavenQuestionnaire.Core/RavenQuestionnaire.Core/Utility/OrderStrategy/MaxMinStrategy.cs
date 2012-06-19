using System.Linq;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Utility.OrderStrategy
{
    public class MaxMinStrategy:IOrderStrategy
    {
        public IEnumerable<T> Reorder<T>(IEnumerable<T> list)
        {
            return list.OrderByDescending(n => (n as Answer).AnswerValue).ToList();
        }
    }
}
