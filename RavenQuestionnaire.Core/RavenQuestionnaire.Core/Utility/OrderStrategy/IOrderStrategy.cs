using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RavenQuestionnaire.Core.Utility.OrderStrategy
{
    public interface IOrderStrategy
    {
        IEnumerable<T> Reorder<T>(IEnumerable<T> list);
        //IEnumerable<T> Reorder<T>(IEnumerable<T> list, Func<T,object> )
    }
}
