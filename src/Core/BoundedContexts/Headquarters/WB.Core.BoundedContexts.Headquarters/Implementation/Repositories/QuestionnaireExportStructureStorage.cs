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
        private readonly IPlainQuestionnaireRepository plainQuestionnaireRepository;
        private readonly IExportViewFactory exportViewFactory;

        private readonly MemoryCache cache = new MemoryCache(nameof(QuestionnaireExportStructure));

        public QuestionnaireExportStructureStorage(IPlainQuestionnaireRepository plainQuestionnaireRepository, IExportViewFactory exportViewFactory)
        {
            this.plainQuestionnaireRepository = plainQuestionnaireRepository;
            this.exportViewFactory = exportViewFactory;
        }

        public QuestionnaireExportStructure GetQuestionnaireExportStructure(QuestionnaireIdentity id)
        {
            var idStringKey = id.ToString();
            var cachedQuestionnaireExportStructure = this.cache.Get(idStringKey);
            if (cachedQuestionnaireExportStructure == null)
            {
                cachedQuestionnaireExportStructure = this.exportViewFactory.CreateQuestionnaireExportStructure(
                    this.plainQuestionnaireRepository.GetQuestionnaireDocument(id), id.Version);

                this.cache.Set(idStringKey, 
                    cachedQuestionnaireExportStructure,
                    DateTime.Now.AddMinutes(5));
            }

            return (QuestionnaireExportStructure) cachedQuestionnaireExportStructure;
        }
    }
}