using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RavenQuestionnaire.Core.Utility.OrderStrategy
{
    public class RandomStrategy:IOrderStrategy
    {
        #region Implementation of IOrderStrategy

        public IEnumerable<T> Reorder<T>(IEnumerable<T> list)
        {
            return list.OrderBy(x => Guid.NewGuid());
        }

        #endregion
    }
}
