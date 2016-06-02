using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Adapters.Latest;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Adapters.V10;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Adapters.V2;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Adapters.V4;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Adapters.V5;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Adapters.V6;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Adapters.V7;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Adapters.V8;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Adapters.V9;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    public class InterviewExpressionStateUpgrader : IInterviewExpressionStateUpgrader
    {
        public ILatestInterviewExpressionState UpgradeToLatestVersionIfNeeded(IInterviewExpressionState state) =>
            InterviewExpressionStateV10ToLatestAdapter.AdaptIfNeeded(
            InterviewExpressionStateV9ToV10Adapter.AdaptIfNeeded(
            InterviewExpressionStateV8ToV9Adapter.AdaptIfNeeded(
            InterviewExpressionStateV7ToV8Adapter.AdaptIfNeeded(
            InterviewExpressionStateV6ToV7Adapter.AdaptIfNeeded(
            InterviewExpressionStateV5ToV6Adapter.AdaptIfNeeded(
            InterviewExpressionStateV4ToV5Adapter.AdaptIfNeeded(
            InterviewExpressionStateV2ToV4Adapter.AdaptIfNeeded(
            InterviewExpressionStateV1ToV2Adapter.AdaptIfNeeded(state)))))))));
    }
}
