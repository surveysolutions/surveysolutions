using System;
using WB.Core.SharedKernels.DataCollection.Aggregates;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Repositories
{
    /// <summary>
    /// Repository of questionnaires which is able to return questionnaire in one of it's previous states.
    /// </summary>
    internal interface IHistoricalQuestionnaireRepository
    {
        IQuestionnaire GetQuestionnaire(Guid id, long version);
    }
}
