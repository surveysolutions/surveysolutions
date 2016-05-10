using System;
using System.Runtime.Caching;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Factories;
using WB.Core.SharedKernels.SurveyManagement.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Repositories
{
    internal class QuestionnaireProjectionsRepository: IQuestionnaireProjectionsRepository
    {
        private readonly IPlainQuestionnaireRepository plainQuestionnaireRepository;
        private readonly IExportViewFactory exportViewFactory;

        private readonly MemoryCache cache = new MemoryCache(nameof(QuestionnaireExportStructure));

        public QuestionnaireProjectionsRepository(IPlainQuestionnaireRepository plainQuestionnaireRepository, IExportViewFactory exportViewFactory)
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
                cachedQuestionnaireExportStructure = exportViewFactory.CreateQuestionnaireExportStructure(
                    this.plainQuestionnaireRepository.GetQuestionnaireDocument(id), id.Version);

                this.cache.Set(idStringKey, 
                    cachedQuestionnaireExportStructure,
                    DateTime.Now.AddMinutes(5));
            }

            return (QuestionnaireExportStructure) cachedQuestionnaireExportStructure;
        }
    }
}