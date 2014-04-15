using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Supervisor.Synchronization.Atom
{
    public interface IAtomFeedReader
    {
        Task<IEnumerable<AtomFeedEntry<T>>> ReadAfterAsync<T>(Uri uri, string lastStoredEntryId);
    }
}