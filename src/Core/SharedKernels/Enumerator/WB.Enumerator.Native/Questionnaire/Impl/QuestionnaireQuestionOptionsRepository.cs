using System;
using System.Collections.Generic;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Enumerator.Native.Questionnaire.Impl
{
    public class QuestionnaireQuestionOptionsRepository : IQuestionOptionsRepository
    {
        private readonly IQuestionnaireStorage questionnaireRepository;

        public QuestionnaireQuestionOptionsRepository(IQuestionnaireStorage questionnaireRepository)
        {
            this.questionnaireRepository = questionnaireRepository;
        }

        public IEnumerable<CategoricalOption> GetOptionsForQuestion(QuestionnaireIdentity questionnaireIdentity, 
            Guid questionId, int? parentQuestionValue, string searchFor, Translation translation)
        {
            var questionnaire = questionnaireRepository.GetQuestionnaire(questionnaireIdentity, translation?.Name);
            
            return questionnaire.GetOptionsForQuestionFromStructure(questionId, parentQuestionValue, searchFor);
        }

        public CategoricalOption GetOptionForQuestionByOptionValue(QuestionnaireIdentity qestionnaireIdentity,
             Guid questionId, decimal optionValue, Translation translation)
        {
            var questionnaire = questionnaireRepository.GetQuestionnaire(qestionnaireIdentity, translation?.Name);
            return questionnaire.GetOptionForQuestionByOptionValueFromStructure(questionId, optionValue);
        }
    }
}
