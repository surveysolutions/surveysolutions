
using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.SharedKernels.DataCollection.Repositories
{
    public interface IQuestionOptionsRepository
    {
        IEnumerable<CategoricalOption> GetOptionsForQuestion(IQuestionnaire questionnaire,
            Guid questionId, int? parentQuestionValue, string searchFor, Translation translationId, int[] excludedOptionIds = null);

        CategoricalOption GetOptionForQuestionByOptionText(IQuestionnaire questionnaire, 
            Guid questionId, string optionText, int? parentQuestionValue, Translation translationId);

        CategoricalOption GetOptionForQuestionByOptionValue(IQuestionnaire questionnaire, 
            Guid questionId, decimal optionValue, int? parentQuestionValue, Translation translationId);

        IEnumerable<CategoricalOption> GetOptionsByOptionValues(IQuestionnaire questionnaire, Guid questionId,
            int[] optionsValues, Translation translation);
    }
}
