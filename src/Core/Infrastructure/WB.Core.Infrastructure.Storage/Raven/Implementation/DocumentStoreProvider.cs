using System;
using System.Linq;
using System.Net;
using Ninject.Activation;
using Raven.Abstractions.Json;
using Raven.Abstractions.Replication;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Embedded;
using Raven.Client.Extensions;
using Raven.Imports.Newtonsoft.Json;
using WB.Core.Infrastructure.Storage.Raven.Implementation.WriteSide;

namespace WB.Core.Infrastructure.Storage.Raven.Implementation
{
    internal class DocumentStoreProvider : Provider<IDocumentStore>
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
        public IDocumentStore CreateSeparateInstanceForEventStore()
        {
            //externalDocumentStore.Conventions = CreateStoreConventions(CollectionName, failoverBehavior);

            return this.settings.IsEmbedded
                ? (IDocumentStore) this.CreateEmbeddedStorage()
                : this.CreateServerStorage(this.settings.EventsDatabase);
        }

        public IDocumentStore CreateInstanceForPlainStorage()
        {
            return this.CreateServerStorage(this.settings.PlainDatabase);
        }

        protected override IDocumentStore CreateInstance(IContext context)
        {
            return this.CreateInstanceForReadSideStore();
        }

        private IDocumentStore CreateInstanceForReadSideStore()
        {
            return this.settings.IsEmbedded
                ? this.GetOrCreateEmbeddedStorage()
                : this.GetOrCreateServerStorage(this.settings.ViewsDatabase);
        }

        private IDocumentStore GetOrCreateServerStorage(string databaseName)
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
                
                Conventions =
                {
                    FailoverBehavior = this.settings.FailoverBehavior,
                    JsonContractResolver = new PropertiesOnlyContractResolver(),
                    CustomizeJsonSerializer = serializer =>
                    {
                        serializer.TypeNameHandling = TypeNameHandling.All;
                        serializer.NullValueHandling = NullValueHandling.Ignore;
                    }
                }
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
            return store;
        }

        private EmbeddableDocumentStore CreateEmbeddedStorage()
        {
            var store = new EmbeddableDocumentStore
            {
                DataDirectory = this.settings.StoragePath,
                UseEmbeddedHttpServer = false,
                Conventions = new DocumentConvention
                {
                    JsonContractResolver = new PropertiesOnlyContractResolver()
                },
                ResourceManagerId = Guid.NewGuid(),
            };
            store.Initialize();

            return store;
        }

        protected internal static DocumentConvention CreateStoreConventions(string ravenCollectionName, FailoverBehavior failoverBehavior = FailoverBehavior.FailImmediately)
        {
            return new DocumentConvention
            {
                FailoverBehavior = failoverBehavior,
                JsonContractResolver = new PropertiesOnlyContractResolver(),
                FindTypeTagName = x => ravenCollectionName,
                CustomizeJsonSerializer = CustomizeJsonSerializer,
            };
        }

        private static void CustomizeJsonSerializer(JsonSerializer serializer)
        {
            SetupSerializerToIgnoreAssemblyNameForEvents(serializer);
        }


        private static void SetupSerializerToIgnoreAssemblyNameForEvents(JsonSerializer serializer)
        {
            serializer.Binder = new IgnoreAssemblyNameForEventsSerializationBinder();

            // if we want to perform serialized type name substitution
            // then JsonDynamicConverter should be removed
            // that is because JsonDynamicConverter handles System.Object types
            // and it by itself does not recognized substituted type
            // and does not allow our custom serialization binder to work
            RemoveJsonDynamicConverter(serializer.Converters);
        }

        private static void RemoveJsonDynamicConverter(JsonConverterCollection converters)
        {
            JsonConverter jsonDynamicConverter = converters.SingleOrDefault(converter => converter is JsonDynamicConverter);

            if (jsonDynamicConverter != null)
            {
                converters.Remove(jsonDynamicConverter);
            }
        }
    }
}
