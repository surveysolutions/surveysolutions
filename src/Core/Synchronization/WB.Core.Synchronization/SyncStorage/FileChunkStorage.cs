using System;
using System.IO;
using Main.Core;
using Newtonsoft.Json;

namespace WB.Core.Synchronization.SyncStorage
{
    public class FileChunkStorage : IChunkStorage
    {
        private readonly string path;
        private const string folderName = "SyncData";

        public FileChunkStorage(string folderPath)
        {
            this.path = Path.Combine(folderPath, folderName);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        private string GetFilePath(Guid id)
        {
            return Path.Combine(this.path, string.Format("{0}.sync", id));
        }

        public void StoreChunk(Guid id, string syncItem)
        {
            File.WriteAllText(GetFilePath(id), syncItem);
        }

        public string ReadChunk(Guid id)
        {
            return File.ReadAllText(GetFilePath(id));
        }
    }
}