using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Aggregates;

namespace WB.Core.SharedKernels.DataCollection.Repositories
{
    public interface IQuestionOptionsRepository
    {
        IEnumerable<CategoricalOption> GetOptionsForQuestion(IQuestionnaire questionnaire, Guid questionId, long? parentQuestionValue, string filter);
    }
}
