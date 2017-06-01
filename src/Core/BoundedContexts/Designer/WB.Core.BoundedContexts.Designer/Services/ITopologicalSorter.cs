using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface ITopologicalSorter<T>
    {
        List<T> Sort(Dictionary<T, T[]> dependencies);

        List<List<T>> DetectCycles(Dictionary<T, T[]> dependencies);
    }
}
