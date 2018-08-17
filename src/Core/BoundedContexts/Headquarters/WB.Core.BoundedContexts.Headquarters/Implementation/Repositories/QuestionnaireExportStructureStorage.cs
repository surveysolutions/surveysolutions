using System;
using System.Runtime.Caching;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Repositories
{
    internal class QuestionnaireExportStructureStorage: IQuestionnaireExportStructureStorage
    {
        private readonly IExportViewFactory exportViewFactory;

        private static readonly MemoryCache cache = new MemoryCache(nameof(QuestionnaireExportStructure));

        public QuestionnaireExportStructureStorage(IExportViewFactory exportViewFactory)
        {
            this.exportViewFactory = exportViewFactory;
        }

        public QuestionnaireExportStructure GetQuestionnaireExportStructure(QuestionnaireIdentity id)
        {
            var idStringKey = id.ToString();
            var cachedQuestionnaireExportStructure = cache.Get(idStringKey);
            if (cachedQuestionnaireExportStructure == null)
            {
                cachedQuestionnaireExportStructure = this.exportViewFactory.CreateQuestionnaireExportStructure(id);

                if (cachedQuestionnaireExportStructure == null)
                    return null;

                cache.Set(idStringKey, 
                    cachedQuestionnaireExportStructure,
                    DateTime.Now.AddHours(1));
            }

            return (QuestionnaireExportStructure) cachedQuestionnaireExportStructure;
        }
    }
}
