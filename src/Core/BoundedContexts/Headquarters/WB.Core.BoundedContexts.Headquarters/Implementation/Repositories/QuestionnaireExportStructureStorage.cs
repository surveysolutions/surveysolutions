using System;
using Microsoft.Extensions.Caching.Memory;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.Infrastructure;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Repositories
{
    internal class QuestionnaireExportStructureStorage: IQuestionnaireExportStructureStorage
    {
        private readonly IExportViewFactory exportViewFactory;
        private readonly IMemoryCache cache;

        public QuestionnaireExportStructureStorage(IExportViewFactory exportViewFactory, IMemoryCache cache)
        {
            this.exportViewFactory = exportViewFactory;
            this.cache = cache;
        }

        public QuestionnaireExportStructure GetQuestionnaireExportStructure(QuestionnaireIdentity id)
        {
            return cache.GetOrCreateNullSafe(id.ToString(), entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(5);
                return this.exportViewFactory.CreateQuestionnaireExportStructure(id);
            });
        }
    }
}
