using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Enumerator.Native.Questionnaire.Impl
{
    public class QuestionnaireQuestionOptionsRepository : IQuestionOptionsRepository
    {
        public IEnumerable<CategoricalOption> GetOptionsForQuestion(IQuestionnaire questionnaire,
            Guid questionId, int? parentQuestionValue, string searchFor, Translation translation,
            int[] excludedOptionIds = null)
        {
            return questionnaire.GetOptionsForQuestionFromStructure(questionId, parentQuestionValue, searchFor, excludedOptionIds);
        }

        public CategoricalOption GetOptionForQuestionByOptionText(IQuestionnaire questionnaire, Guid questionId, string optionText, int? parentQuestionValue, Translation translation)
        {
            return questionnaire.GetOptionForQuestionByOptionTextFromStructure(questionId, optionText, parentQuestionValue);
        }

        public CategoricalOption GetOptionForQuestionByOptionValue(IQuestionnaire questionnaire,
             Guid questionId, decimal optionValue, int? parentQuestionValue, Translation translation)
        {
            return questionnaire.GetOptionForQuestionByOptionValueFromStructure(questionId, optionValue, parentQuestionValue);
        }

        public IEnumerable<CategoricalOption> GetOptionsByOptionValues(IQuestionnaire questionnaire, Guid questionId, int[] optionsValues, Translation translation) => 
            questionnaire.GetOptionsForQuestion(questionId, null, null, null).Where(x => optionsValues.Contains(x.Value));
    }
}
