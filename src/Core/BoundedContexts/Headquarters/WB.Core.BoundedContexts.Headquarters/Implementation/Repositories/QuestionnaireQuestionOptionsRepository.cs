using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Repositories
{
    public class QuestionnaireQuestionOptionsRepository : IQuestionOptionsRepository
    {
        private readonly IQuestionnaireStorage questionnaireRepository;

        public QuestionnaireQuestionOptionsRepository(IQuestionnaireStorage questionnaireRepository)
        {
            this.questionnaireRepository = questionnaireRepository;
        }

        public IEnumerable<CategoricalOption> GetOptionsForQuestion(QuestionnaireIdentity qestionnaireIdentity, 
            Guid questionId, int? parentQuestionValue, string filter, Translation translation)
        {
            var questionnaire = questionnaireRepository.GetQuestionnaire(qestionnaireIdentity, translation?.Name);
            
            return questionnaire.GetOptionsForQuestionFromStructure(questionId, parentQuestionValue, filter);
        }

        public CategoricalOption GetOptionForQuestionByOptionText(QuestionnaireIdentity qestionnaireIdentity, Guid questionId, string optionText, int? parentQuestionValue, Translation translation)
        {
            var questionnaire = questionnaireRepository.GetQuestionnaire(qestionnaireIdentity, translation?.Name);

            return questionnaire.GetOptionForQuestionByOptionTextFromStructure(questionId, optionText, parentQuestionValue);
        }

        public CategoricalOption GetOptionForQuestionByOptionValue(QuestionnaireIdentity qestionnaireIdentity,
             Guid questionId, decimal optionValue, Translation translation)
        {
            var questionnaire = questionnaireRepository.GetQuestionnaire(qestionnaireIdentity, translation?.Name);

            return questionnaire.GetOptionForQuestionByOptionValueFromStructure(questionId, optionValue);
        }
    }
}
