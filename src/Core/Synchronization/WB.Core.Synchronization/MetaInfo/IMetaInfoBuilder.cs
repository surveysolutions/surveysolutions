using Main.Core.Documents;
using WB.Core.SharedKernel.Structures.Synchronization;

namespace WB.Core.Synchronization.MetaInfo
{
    public interface IMetaInfoBuilder
    {
        InterviewMetaInfo GetInterviewMetaInfo(CompleteQuestionnaireDocument doc);
    }
}
