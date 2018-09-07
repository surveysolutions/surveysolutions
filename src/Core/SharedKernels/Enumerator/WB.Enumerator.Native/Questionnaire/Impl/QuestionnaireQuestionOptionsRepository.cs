using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Enumerator.Native.Questionnaire.Impl
{
    public class QuestionnaireQuestionOptionsRepository : IQuestionOptionsRepository
    {
        private IQuestionnaire questionnaire;

        //has reference from cache and can't have link to scoped IQuestionnaireStorage

        //private readonly IQuestionnaireStorage questionnaireRepository;
        
        public void SetCurentQuestionnaire(IQuestionnaire questionnaire)
        {
            this.questionnaire = questionnaire;
        }

        public QuestionnaireQuestionOptionsRepository(/*IQuestionnaireStorage questionnaireRepository*/)
        {
            //this.questionnaireRepository = questionnaireRepository;
        }

        public IEnumerable<CategoricalOption> GetOptionsForQuestion(QuestionnaireIdentity qestionnaireIdentity, 
            Guid questionId, int? parentQuestionValue, string searchFor, Translation translation)
        {
            //var questionnaire = questionnaireRepository.GetQuestionnaire(qestionnaireIdentity, translation?.Name);
            
            return questionnaire.GetOptionsForQuestionFromStructure(questionId, parentQuestionValue, searchFor);
        }

        public CategoricalOption GetOptionForQuestionByOptionText(QuestionnaireIdentity qestionnaireIdentity, Guid questionId, string optionText, int? parentQuestionValue, Translation translation)
        {
            //var questionnaire = questionnaireRepository.GetQuestionnaire(qestionnaireIdentity, translation?.Name);

            return questionnaire.GetOptionForQuestionByOptionTextFromStructure(questionId, optionText, parentQuestionValue);
        }

        public CategoricalOption GetOptionForQuestionByOptionValue(QuestionnaireIdentity qestionnaireIdentity,
             Guid questionId, decimal optionValue, Translation translation)
        {
            //var questionnaire = questionnaireRepository.GetQuestionnaire(qestionnaireIdentity, translation?.Name);

            return questionnaire.GetOptionForQuestionByOptionValueFromStructure(questionId, optionValue);
        }
    }
}
