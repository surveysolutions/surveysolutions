using System;
using System.IO;
using WB.Core.Infrastructure;
using WB.Core.SharedKernel.Utils.Serialization;

namespace WB.Core.BoundedContexts.Supervisor.Implementation.TemporaryDataStorage
{
    internal class FileTemporaryDataStorage<T> : ITemporaryDataStorage<T> where T : class
    {
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
            var path = GetOrCreateObjectStoreFolder();
            File.WriteAllText(GetItemFileName(path, name), jsonSerrializer.GetItemAsContent(payload));
        }

        public T GetByName(string name)
        {
            var path = GetOrCreateObjectStoreFolder();
            var fullFilePath = Path.Combine(path, name);
            if (!File.Exists(fullFilePath))
                return null;
            var fileContent = File.ReadAllText(fullFilePath);

            return jsonSerrializer.Deserrialize<T>(fileContent);
        }

        private string GetOrCreateObjectStoreFolder()
        {
            var path = Path.Combine(rootPath, typeof (T).Name);
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
