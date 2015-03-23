using System.Linq;
using Ninject.Modules;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.FileSystem;
using WB.Core.Infrastructure.Storage.Raven.Implementation;

namespace WB.Core.Infrastructure.Storage.Raven
{
    public abstract class RavenInfrastructureModule : NinjectModule
    {
        private readonly RavenConnectionSettings settings;

        protected RavenInfrastructureModule(RavenConnectionSettings settings)
        {
            this.settings = settings;
        }

        protected void BindDocumentStore()
        {
            if (this.IsDocumentStoreAlreadyBound())
                return;

            var storeProvider = new DocumentStoreProvider(this.settings);
            this.Bind<RavenConnectionSettings>().ToSelf();
            this.Bind<DocumentStoreProvider>().ToConstant(storeProvider);
            this.Bind<IDocumentStore>().ToProvider<DocumentStoreProvider>();

            var fileStoreProvider = new FileStoreProvider(this.settings);
            this.Bind<FileStoreProvider>().ToConstant(fileStoreProvider);
            this.Bind<IFilesStore>().ToProvider<FileStoreProvider>();
        }

        private bool IsDocumentStoreAlreadyBound()
        {
            return this.Kernel.GetBindings(typeof(IDocumentStore)).Any();
        }
    }
}