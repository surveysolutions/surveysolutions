using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.Synchronization.SyncStorage
{
    public interface IChunkStorageFactory
    {
        IChunkStorage GetStorage(Guid supervisorId);
    }
}
