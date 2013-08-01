using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.SharedKernel.Structures.Synchronization;

namespace WB.Core.Synchronization.SyncStorage
{
    public interface IChunkReader
    {
        SyncItem ReadChunk(Guid id);
        IEnumerable<Guid> GetChunksCreatedAfterForUsers(long sequence, IEnumerable<Guid> users);

        IEnumerable<KeyValuePair<long, Guid>> GetChunkPairsCreatedAfter(long sequence, IEnumerable<Guid> users);
    }
}
