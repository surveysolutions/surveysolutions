using System.Collections.Generic;
using System.Reflection;
using Raven.Abstractions.Data;
using Raven.Abstractions.Linq;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Extensions;
using Raven.Client.Indexes;
using Raven.Json.Linq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.Infrastructure.Storage.Raven.Implementation.ReadSide.RepositoryAccessors
{
    internal class ReadSideCleaner : IReadSideCleaner
    {
        private readonly DocumentStoreProvider documentStoreProvider;
        private readonly Assembly[] assembliesWithIndexes;

        public ReadSideCleaner(DocumentStoreProvider documentStoreProvider, Assembly[] assembliesWithIndexes)
        {
            this.documentStoreProvider = documentStoreProvider;
            this.assembliesWithIndexes = assembliesWithIndexes;
        }

        public void ReCreateViewDatabase()
        {
            documentStoreProvider.RemoveInstanceForReadSideStore();
            documentStoreProvider.CreateInstanceForReadSideStore();
        }

        public void CreateIndexesAfterRebuildReadSide()
        {
            var documentStore = documentStoreProvider.CreateInstanceForReadSideStore();

            foreach (Assembly assembly in this.assembliesWithIndexes)
            {
                IndexCreation.CreateIndexes(assembly, documentStore);
            }
        }
    }
}
