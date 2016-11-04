using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;

namespace WB.Core.SharedKernels.DataCollection.Services
{
    public interface ISubstitionTextFactory
    {
        SubstitionText CreateText(Identity identity, string text, IQuestionnaire questionnaire);
    }
}