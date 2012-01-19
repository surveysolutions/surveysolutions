using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RavenQuestionnaire.Core.Entities.Iterators
{
    public interface Iterator<T> : IEnumerable<T>, IEnumerator<T>
    {
     /*   T First { get; }
        T Last { get; }*/
        T Next { get; }
        T Previous { get; }
       /* bool IsDone { get; }*/
        void SetCurrent(T item);
    }
}
