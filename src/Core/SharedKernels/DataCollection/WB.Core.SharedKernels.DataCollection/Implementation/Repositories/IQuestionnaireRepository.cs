using System;
using WB.Core.SharedKernels.DataCollection.Aggregates;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Repositories
{
    internal interface IQuestionnaireRepository
    {
        /// <summary>
        /// Returns latest version of questionnaire.
        /// </summary>
        IQuestionnaire GetQuestionnaire(Guid id);

        /// <summary>
        /// Returns questionnaire in one of it's previous states specified by version.
        /// </summary>
        IQuestionnaire GetHistoricalQuestionnaire(Guid id, long version);
    }
}
