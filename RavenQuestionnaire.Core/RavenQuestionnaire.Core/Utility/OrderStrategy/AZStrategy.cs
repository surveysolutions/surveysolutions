using System.Linq;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Utility.OrderStrategy
{
    public class AZStrategy:IOrderStrategy
    {
        public IEnumerable<T> Reorder<T>(IEnumerable<T> list)
        {
            return list.OrderBy(n => (n as Answer).AnswerText);
        }
    }
}
