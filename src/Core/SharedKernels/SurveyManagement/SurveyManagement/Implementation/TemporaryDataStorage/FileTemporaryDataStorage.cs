using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using WB.Core.Infrastructure;
using WB.Core.SharedKernel.Utils.Serialization;

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
        public FileTemporaryDataStorage(IJsonUtils jsonSerrializer)
            : this(jsonSerrializer, AppDomain.CurrentDomain.GetData("DataDirectory").ToString())
        {
        }

        public void Store(T payload, string name)
        {
            var path = this.GetOrCreateObjectStoreFolder();

            lock (GetLockObject(path))
            {
                File.WriteAllText(this.GetItemFileName(path, name), this.jsonSerrializer.GetItemAsContent(payload));
            }
        }

        public T GetByName(string name)
        {
            var path = this.GetOrCreateObjectStoreFolder();
            var fullFilePath = Path.Combine(path, name);
            if (!File.Exists(fullFilePath))
                return null;

            lock (GetLockObject(fullFilePath))
            {
                var fileContent = File.ReadAllText(fullFilePath);
                return this.jsonSerrializer.Deserrialize<T>(fileContent);
            }
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
