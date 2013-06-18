using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.Synchronization.SyncStorage
{
    public class DefaultChunkStorageFactory : IChunkStorageFactory
    {
        private const string FolderName = "SyncData";
        private readonly string path;
        private readonly IDictionary<Guid, IChunkStorage> chunkStorages; 

        public DefaultChunkStorageFactory(string folderPath)
        {
            this.chunkStorages=new Dictionary<Guid, IChunkStorage>();
            this.path = Path.Combine(folderPath, FolderName);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        public IChunkStorage GetStorage(Guid supervisorId)
        {
            if (!chunkStorages.ContainsKey(supervisorId))

                chunkStorages[supervisorId] = new FileChunkStorage(this.path, supervisorId);
            return chunkStorages[supervisorId];

        }
    }
}
