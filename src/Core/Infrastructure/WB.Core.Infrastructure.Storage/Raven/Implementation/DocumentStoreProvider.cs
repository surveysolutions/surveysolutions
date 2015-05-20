using System;
using System.Net;
using System.Net.Http;
using Ninject.Activation;
using Raven.Client;
using Raven.Client.Document;
using Raven.Imports.Newtonsoft.Json;

namespace WB.Core.Infrastructure.Storage.Raven.Implementation
{
    internal class DocumentStoreProvider : Provider<IDocumentStore>
    {
        private readonly RavenConnectionSettings settings;
        
        private DocumentStore serverStorage;

        public DocumentStoreProvider(RavenConnectionSettings settings)
        {
            if (settings == null) throw new ArgumentNullException("settings");
            this.settings = settings;
        }

        public IDocumentStore CreateInstanceForPlainStorage()
        {
            return this.CreateServerStorage(this.settings.PlainDatabase);
        }

        protected override IDocumentStore CreateInstance(IContext context)
        {
            return this.CreateInstanceForReadSideStore();
        }

        public IDocumentStore CreateInstanceForReadSideStore()
        {
            return this.GetOrCreateServerStorage(this.settings.ViewsDatabase);
        }

        public void RemoveInstanceForReadSideStore()
        {
            serverStorage.DatabaseCommands.GlobalAdmin.DeleteDatabase(this.settings.ViewsDatabase, hardDelete: true);
            serverStorage = null;
        }

        private IDocumentStore GetOrCreateServerStorage(string databaseName)
        {
            return this.serverStorage ?? (this.serverStorage = this.CreateServerStorage(databaseName));
        }

        private DocumentStore CreateServerStorage(string databaseName)
        {
            
            var store = new DocumentStore
            {
                Url = this.settings.StoragePath,
                DefaultDatabase = databaseName,
                Conventions =
                {
                    FailoverBehavior = this.settings.FailoverBehavior,
                    JsonContractResolver = new PropertiesOnlyContractResolver(),
                    CustomizeJsonSerializer = serializer =>
                    {
                        serializer.TypeNameHandling = TypeNameHandling.All;
                        serializer.NullValueHandling = NullValueHandling.Ignore;
                    }
                },
            };

            if (!string.IsNullOrWhiteSpace(this.settings.Username))
            {
                store.Credentials = new NetworkCredential(this.settings.Username, this.settings.Password);
            }

            store.Initialize(true);

            if (!string.IsNullOrWhiteSpace(databaseName))
            {
                var systemStorage = new DocumentStore
                {
                    Url = this.settings.StoragePath
                };
                systemStorage.Initialize();
                systemStorage.ActivateBundles(this.settings.ActiveBundles, databaseName);
            }
            else
            {
                store.ActivateBundles(this.settings.ActiveBundles, databaseName);
            }

            var webRequestHandler = new WebRequestHandler();
            webRequestHandler.UnsafeAuthenticatedConnectionSharing = true;
            webRequestHandler.PreAuthenticate = true;

            store.HttpMessageHandler = webRequestHandler;

            return store;
        }
    }
}
