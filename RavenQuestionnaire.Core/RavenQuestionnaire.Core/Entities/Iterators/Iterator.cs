using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RavenQuestionnaire.Core.Entities.Iterators
{
    public interface Iterator<T>
    {
        T First { get; }
        T Next { get; }
        T Previous { get; }
        bool IsDone { get; }
        T CurrentItem { get; }
    }
}
