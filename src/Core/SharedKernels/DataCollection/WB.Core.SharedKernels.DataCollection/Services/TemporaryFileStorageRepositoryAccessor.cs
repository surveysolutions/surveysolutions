using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.SharedKernel.Utils.Serialization;

namespace WB.Core.SharedKernels.DataCollection.Services
{
    public class TemporaryFileStorageRepositoryAccessor
    {
        private readonly string rootPath;
        private readonly IJsonUtils jsonSerrializer;

        public TemporaryFileStorageRepositoryAccessor(string rootPath, IJsonUtils jsonSerrializer)
        {
            this.rootPath = rootPath;
            this.jsonSerrializer = jsonSerrializer;
        }

        public void Store<T>(T payload, string name) where T : class 
        {
            var path = GetOrCreateObjectStoreFolder<T>();
            File.WriteAllText(GetItemFileName(path, name), jsonSerrializer.GetItemAsContent(payload));
        }

        public T GetByName<T>(string name) where T :class 
        {
            var path = GetOrCreateObjectStoreFolder<T>();
            var fullFilePath = Path.Combine(path, name);
            if (!File.Exists(fullFilePath))
                return null;
            var fileContent = File.ReadAllText(fullFilePath);

            return jsonSerrializer.Deserrialize<T>(fileContent);
        }

        private string GetOrCreateObjectStoreFolder<T>()
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
