﻿using System;
using System.Net;
using Ninject.Activation;
using Ninject.Planning.Targets;
using Raven.Client.Document;
using Raven.Client.Embedded;
using Raven.Client.Extensions;
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
            if (settings == null) throw new ArgumentNullException("settings");
            this.settings = settings;
        }

        /// <summary>
        /// Creates a separate instance for event store.
        /// This is needed because event store substitutes conventions and substituted are not compatible with read side.
        /// Always creates a new instance, so should be called only once per app.
        /// </summary>
        public DocumentStore CreateSeparateInstanceForEventStore()
        {
            return this.settings.IsEmbedded
                ? this.CreateEmbeddedStorage()
                : this.CreateServerStorage(this.settings.EventsDatabase);
        }

        public DocumentStore CreateInstanceForPlainStorage()
        {
            return this.CreateServerStorage(this.settings.PlainDatabase);
        }

        protected override DocumentStore CreateInstance(IContext context)
        {
            return this.CreateInstanceForReadSideStore();
        }

        private DocumentStore CreateInstanceForReadSideStore()
        {
            return this.settings.IsEmbedded
                ? this.GetOrCreateEmbeddedStorage()
                : this.GetOrCreateServerStorage(this.settings.ViewsDatabase);
        }

        private DocumentStore GetOrCreateServerStorage(string databaseName)
        {
            return this.serverStorage ?? (this.serverStorage = this.CreateServerStorage(databaseName));
        }

        private EmbeddableDocumentStore GetOrCreateEmbeddedStorage()
        {
            if (this.embeddedStorage == null || this.embeddedStorage.WasDisposed)
            {
                this.embeddedStorage = this.CreateEmbeddedStorage();
            }

            return this.embeddedStorage;
        }

        private DocumentStore CreateServerStorage(string databaseName)
        {
            var store = new DocumentStore
                {
                    Url = this.settings.StoragePath,
                    DefaultDatabase = databaseName,
                    Conventions = {JsonContractResolver = new PropertiesOnlyContractResolver()}
                };

            if (!string.IsNullOrWhiteSpace(this.settings.Username))
            {
                store.Credentials = new NetworkCredential(this.settings.Username, this.settings.Password);
            }

            store.Initialize();

            if (!string.IsNullOrWhiteSpace(databaseName))
            {
                store.DatabaseCommands.EnsureDatabaseExists(databaseName);
            }

            return store;
        }

        private EmbeddableDocumentStore CreateEmbeddedStorage()
        {
            var store = new EmbeddableDocumentStore
            {
                DataDirectory = this.settings.StoragePath,
                UseEmbeddedHttpServer = false,
                Conventions = new DocumentConvention {JsonContractResolver = new PropertiesOnlyContractResolver()},
                ResourceManagerId = Guid.NewGuid(),
            };

            store.Initialize();

            return store;
        }
    }
}
