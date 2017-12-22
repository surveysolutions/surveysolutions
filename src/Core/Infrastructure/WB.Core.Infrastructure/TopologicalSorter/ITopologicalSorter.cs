using System.Collections.Generic;

namespace WB.Core.Infrastructure.TopologicalSorter
{
    public interface ITopologicalSorter<T>
    {
        List<T> Sort(Dictionary<T, T[]> dependencies);

        List<List<T>> DetectCycles(Dictionary<T, T[]> dependencies);
    }
}
