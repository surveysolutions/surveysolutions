using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject.Activation;
using Raven.Client;
using Raven.Client.FileSystem;

namespace WB.Core.Infrastructure.Storage.Raven.Implementation
{
    internal class FileStoreProvider : Provider<FilesStore>
    {
        private readonly RavenConnectionSettings settings;

        public FileStoreProvider(RavenConnectionSettings settings)
        {
            this.settings = settings;
        }

        protected override FilesStore CreateInstance(IContext context)
        {
            var store=
                new FilesStore()
                {
                    Url = settings.StoragePath,
                    DefaultFileSystem = settings.RavenFileSystemName, MaxNumberOfCachedRequests = 2048
                };
            
            store.Initialize(true);
            return store;
        }
    }
}
