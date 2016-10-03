using System;
using System.Runtime.Caching;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Repositories
{
    internal class QuestionnaireExportStructureStorage: IQuestionnaireExportStructureStorage
    {
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IExportViewFactory exportViewFactory;

        private readonly MemoryCache cache = new MemoryCache(nameof(QuestionnaireExportStructure));

        public QuestionnaireExportStructureStorage(IQuestionnaireStorage questionnaireStorage, IExportViewFactory exportViewFactory)
        {
            this.questionnaireStorage = questionnaireStorage;
            this.exportViewFactory = exportViewFactory;
        }

        public QuestionnaireExportStructure GetQuestionnaireExportStructure(QuestionnaireIdentity id)
        {
            var idStringKey = id.ToString();
            var cachedQuestionnaireExportStructure = this.cache.Get(idStringKey);
            if (cachedQuestionnaireExportStructure == null)
            {
                cachedQuestionnaireExportStructure = this.exportViewFactory.CreateQuestionnaireExportStructure(id);

                this.cache.Set(idStringKey, 
                    cachedQuestionnaireExportStructure,
                    DateTime.Now.AddMinutes(5));
            }

            return (QuestionnaireExportStructure) cachedQuestionnaireExportStructure;
        }
    }
}