using System;
using System.Runtime.Caching;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.SharedKernels.DataCollection.Factories;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Repositories
{
    internal class QuestionnaireRosterStructureStorage : IQuestionnaireRosterStructureStorage
    {
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IQuestionnaireRosterStructureFactory questionnaireRosterStructureFactory;

        private readonly MemoryCache cache = new MemoryCache(nameof(QuestionnaireRosterStructure));

        public QuestionnaireRosterStructureStorage(IQuestionnaireStorage questionnaireStorage, IQuestionnaireRosterStructureFactory questionnaireRosterStructureFactory)
        {
            this.questionnaireStorage = questionnaireStorage;
            this.questionnaireRosterStructureFactory = questionnaireRosterStructureFactory;
        }

        public QuestionnaireRosterStructure GetQuestionnaireRosterStructure(QuestionnaireIdentity id)
        {
            var idStringKey = id.ToString();
            var cachedQuestionnaireRosterStructure = this.cache.Get(idStringKey);
            if (cachedQuestionnaireRosterStructure == null)
            {
                cachedQuestionnaireRosterStructure = this.questionnaireRosterStructureFactory.CreateQuestionnaireRosterStructure(
                    this.questionnaireStorage.GetQuestionnaireDocument(id), id.Version);

                this.cache.Set(idStringKey,
                    cachedQuestionnaireRosterStructure,
                    DateTime.Now.AddMinutes(5));
            }

            return (QuestionnaireRosterStructure)cachedQuestionnaireRosterStructure;
        }
    }
}