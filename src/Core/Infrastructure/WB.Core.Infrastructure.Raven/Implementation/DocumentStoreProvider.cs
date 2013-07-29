using WB.Core.Infrastructure.Raven;
using WB.Core.Infrastructure.Raven.Implementation.WriteSide;

namespace Main.Core
{
    using System;
    using System.Net;

    using Ninject.Activation;

    using Raven.Client.Document;
    using Raven.Client.Embedded;
    using Raven.Client.Extensions;
    using Ncqrs.Eventing.Storage.RavenDB;

    internal class DocumentStoreProvider : Provider<DocumentStore>
    {
        private readonly RavenConnectionSettings settings;

        private EmbeddableDocumentStore embeddedStorage;

        public DocumentStoreProvider(RavenConnectionSettings settings)
        {
            this.settings = settings;
        }

        protected override DocumentStore CreateInstance(IContext context)
        {
            DocumentStore store = this.settings.IsEmbedded ? this.GetEmbededStorage() : this.GetServerStorage();

            store.Initialize();

            return store;
        }

        private DocumentStore GetServerStorage()
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
            }

            return this.embeddedStorage;
        }
    }
}
