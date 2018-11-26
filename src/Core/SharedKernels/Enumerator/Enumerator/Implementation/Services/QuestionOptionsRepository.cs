using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    public class QuestionOptionsRepository : IQuestionOptionsRepository
    {
        private IOptionsRepository optionsRepository;

        public QuestionOptionsRepository(IOptionsRepository optionsRepository)
        {
            if(optionsRepository == null)
                throw new ArgumentException(nameof(optionsRepository));

            this.optionsRepository = optionsRepository;
        }

        public IEnumerable<CategoricalOption> GetOptionsForQuestion(IQuestionnaire questionnaire,
            Guid questionId, int? parentQuestionValue, string filter, Translation translation)
        {
            return this.optionsRepository.GetFilteredQuestionOptions(new QuestionnaireIdentity(questionnaire.QuestionnaireId, questionnaire.Version), questionId, parentQuestionValue, filter, translation?.Id);
        }

        public CategoricalOption GetOptionForQuestionByOptionText(IQuestionnaire questionnaire, Guid questionId, string optionText, int? parentQuestionValue, Translation translation)
        {
            return this.optionsRepository.GetQuestionOption(new QuestionnaireIdentity(questionnaire.QuestionnaireId, questionnaire.Version), questionId, optionText, parentQuestionValue, translation?.Id);
        }

        public CategoricalOption GetOptionForQuestionByOptionValue(IQuestionnaire questionnaire,
            Guid questionId, decimal optionValue, Translation translation)
        {
            return this.optionsRepository.GetQuestionOptionByValue(new QuestionnaireIdentity(questionnaire.QuestionnaireId, questionnaire.Version), questionId, optionValue, translation?.Id);
        }
    }
}
