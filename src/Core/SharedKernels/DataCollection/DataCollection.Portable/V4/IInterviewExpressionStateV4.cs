using WB.Core.SharedKernels.DataCollection.V2;

namespace WB.Core.SharedKernels.DataCollection.V4
{
    public interface IInterviewExpressionStateV4 : IInterviewExpressionStateV2
    {
        void SetInterviewProperties(IInterviewProperties properties);

        new IInterviewExpressionStateV4 Clone();
    }
}