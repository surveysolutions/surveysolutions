using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WB.Core.GenericSubdomains.Utils.Implementation.TopologicalSorter;

namespace WB.Core.GenericSubdomains.Utils
{
    public interface ITopologicalSorter<T>
    {
        List<T> Sort(Dictionary<T, T[]> dependencies);
    }
}
