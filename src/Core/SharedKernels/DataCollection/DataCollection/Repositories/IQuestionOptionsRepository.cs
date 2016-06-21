using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Aggregates;

namespace WB.Core.SharedKernels.DataCollection.Repositories
{
    public interface IQuestionOptionsRepository
    {
        IEnumerable<CategoricalOption> GetOptionsForQuestion(IQuestionnaire questionnaire, Guid questionId, int? parentQuestionValue, string filter);

        CategoricalOption GetOptionForQuestionByOptionText(IQuestionnaire questionnaire, Guid questionId, string optionText);
    }
}
