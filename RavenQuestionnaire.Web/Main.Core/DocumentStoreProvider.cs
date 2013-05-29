#if !MONODROID

namespace Main.Core
{
    using System;
    using System.Net;

    using Ninject.Activation;

    using Raven.Client.Document;
    using Raven.Client.Embedded;
    using Raven.Client.Extensions;

    public class DocumentStoreProvider : Provider<DocumentStore>
    {
        private readonly bool isEmbedded;
        private readonly string username;
        private readonly string password;
        private readonly string storagePath;

        private EmbeddableDocumentStore embeddedStorage;

        public DocumentStoreProvider(string storagePath, bool isEmbedded, string username, string password)
        {
            this.storagePath = storagePath;
            this.isEmbedded = isEmbedded;
            this.username = username;
            this.password = password;
        }

        protected override DocumentStore CreateInstance(IContext context)
        {
            DocumentStore store = this.isEmbedded ? this.GetEmbededStorage() : this.GetServerStorage();

            store.Initialize();

            return store;
        }

        private DocumentStore GetServerStorage()
        {
            bool shouldSupplyCredentials = !string.IsNullOrWhiteSpace(this.username);

            return shouldSupplyCredentials
                ? new DocumentStore { Url = this.storagePath, Credentials = new NetworkCredential(this.username, this.password) }
                : new DocumentStore { Url = this.storagePath };
        }

        private EmbeddableDocumentStore GetEmbededStorage()
        {
            if (!isEmbedded)
                throw new InvalidOperationException("You can't call this method");

            if (this.embeddedStorage == null || this.embeddedStorage.WasDisposed)
            {
                this.embeddedStorage = new EmbeddableDocumentStore() { DataDirectory = this.storagePath, UseEmbeddedHttpServer = false };
                this.embeddedStorage.ResourceManagerId = Guid.NewGuid();
            }

            return this.embeddedStorage;
        }
    }
}

#endif
