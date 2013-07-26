#if !MONODROID

namespace Main.Core
{
    using System;
    using System.Net;

    using Ninject.Activation;

    using Raven.Client.Document;
    using Raven.Client.Embedded;
    using Raven.Client.Extensions;
    using Ncqrs.Eventing.Storage.RavenDB;

    public class DocumentStoreProvider : Provider<DocumentStore>
    {
        private readonly bool isEmbedded;
        private readonly string username;
        private readonly string password;
        private readonly string storagePath;
        private readonly string defaultDatabase;

        private EmbeddableDocumentStore embeddedStorage;

        public DocumentStoreProvider(string storagePath, string defaultDatabase, bool isEmbedded, string username, string password)
        {
            this.storagePath = storagePath;
            this.isEmbedded = isEmbedded;
            this.username = username;
            this.password = password;
            this.defaultDatabase = defaultDatabase;
        }

        protected override DocumentStore CreateInstance(IContext context)
        {
            DocumentStore store = this.isEmbedded ? this.GetEmbededStorage() : this.GetServerStorage();

            store.Initialize();

            return store;
        }

        private DocumentStore GetServerStorage()
        {
            var store = new DocumentStore
                {
                    Url = this.storagePath,
                    DefaultDatabase = this.defaultDatabase,
                    Conventions = {JsonContractResolver = new PropertiesOnlyContractResolver()}
                };

            if (!string.IsNullOrWhiteSpace(this.username))
            {
                store.Credentials = new NetworkCredential(this.username, this.password);
            }

            return store;
        }

        private EmbeddableDocumentStore GetEmbededStorage()
        {
            if (!isEmbedded)
                throw new InvalidOperationException("You can't call this method");

            if (this.embeddedStorage == null || this.embeddedStorage.WasDisposed)
            {
                this.embeddedStorage = new EmbeddableDocumentStore()
                    {
                        DataDirectory = this.storagePath,
                        UseEmbeddedHttpServer = false,
                        Conventions = new DocumentConvention() { JsonContractResolver = new PropertiesOnlyContractResolver() }
                    };
                this.embeddedStorage.ResourceManagerId = Guid.NewGuid();
            }

            return this.embeddedStorage;
        }
    }
}

#endif
