using Main.Core.Documents;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;

namespace WB.Core.Synchronization.MetaInfo
{
    public interface IMetaInfoBuilder
    {
        InterviewMetaInfo GetInterviewMetaInfo(InterviewSynchronizationDto doc);
    }
}
