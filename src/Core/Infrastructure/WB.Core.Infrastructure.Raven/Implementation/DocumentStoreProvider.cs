using System;
using System.Net;
using Ninject.Activation;
using Raven.Client.Document;
using Raven.Client.Embedded;
using WB.Core.Infrastructure.Raven.Implementation.WriteSide;

namespace WB.Core.Infrastructure.Raven.Implementation
{
    internal class DocumentStoreProvider : Provider<DocumentStore>
    {
        private readonly RavenConnectionSettings settings;

        private EmbeddableDocumentStore embeddedStorage;
        private DocumentStore serverStorage;

        public DocumentStoreProvider(RavenConnectionSettings settings)
        {
            this.settings = settings;
        }

        protected override DocumentStore CreateInstance(IContext context)
        {
            DocumentStore store = this.settings.IsEmbedded ? this.GetEmbededStorage() : this.GetServerStorage();

            return store;
        }

        private DocumentStore GetServerStorage()
        {
            return this.serverStorage ?? (this.serverStorage = this.GetServerStorageImpl());
        }

        private DocumentStore GetServerStorageImpl()
        {
            var store = new DocumentStore
                {
                    Url = this.settings.StoragePath,
                    DefaultDatabase = this.settings.DefaultDatabase,
                    Conventions = {JsonContractResolver = new PropertiesOnlyContractResolver()}
                };

            if (!string.IsNullOrWhiteSpace(this.settings.Username))
            {
                store.Credentials = new NetworkCredential(this.settings.Username, this.settings.Password);
            }

            store.Initialize();

            return store;
        }

        private EmbeddableDocumentStore GetEmbededStorage()
        {
            if (!this.settings.IsEmbedded)
                throw new InvalidOperationException("You can't call this method");

            if (this.embeddedStorage == null || this.embeddedStorage.WasDisposed)
            {
                this.embeddedStorage = new EmbeddableDocumentStore()
                    {
                        DataDirectory = this.settings.StoragePath,
                        UseEmbeddedHttpServer = false,
                        Conventions = new DocumentConvention() { JsonContractResolver = new PropertiesOnlyContractResolver() }
                    };
                this.embeddedStorage.ResourceManagerId = Guid.NewGuid();

                this.embeddedStorage.Initialize();
            }

            return this.embeddedStorage;
        }
    }
}
