using System;
using System.Collections.Concurrent;
using System.IO;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.TemporaryDataStorage
{
    internal class FileTemporaryDataStorage<T> : ITemporaryDataStorage<T> where T : class
    {
        static ConcurrentDictionary<string, object> lockedFiles = new ConcurrentDictionary<string, object>();
        private readonly string rootPath;
        private readonly IJsonUtils jsonSerrializer;

        public FileTemporaryDataStorage(IJsonUtils jsonSerrializer, string rootPath)
        {
            this.rootPath = rootPath;
            this.jsonSerrializer = jsonSerrializer;
        }
       
        public void Store(T payload, string name)
        {
            var path = this.GetOrCreateObjectStoreFolder();

            string fullFilePath = this.GetItemFileName(path, name);
            lock (GetLockObject(fullFilePath))
            {
                File.WriteAllText(fullFilePath, this.jsonSerrializer.Serialize(payload));
            }
        }

        public T GetByName(string name)
        {
            var path = this.GetOrCreateObjectStoreFolder();
            var fullFilePath = Path.Combine(path, name);
            if (!File.Exists(fullFilePath))
                return null;

            string fileContent;
            lock (GetLockObject(fullFilePath))
            {
                fileContent = File.ReadAllText(fullFilePath);    
            }

            return this.jsonSerrializer.Deserialize<T>(fileContent);
        }

        private static object GetLockObject(string path)
        {
            if (!lockedFiles.ContainsKey(path))
                lockedFiles[path] = new object();

            return lockedFiles[path];
        }

        private string GetOrCreateObjectStoreFolder()
        {
            var path = Path.Combine(this.rootPath, typeof(T).Name);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }

        private string GetItemFileName(string folderPath, string name)
        {
            return Path.Combine(folderPath, name);
        }
    }
}
