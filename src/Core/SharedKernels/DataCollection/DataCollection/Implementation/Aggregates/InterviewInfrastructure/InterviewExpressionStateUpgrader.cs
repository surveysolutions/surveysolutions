using System;
using System.Runtime.InteropServices;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Adapters;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Adapters.Latest;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Adapters.V2;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Adapters.V4;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Adapters.V5;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Adapters.V6;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Adapters.V7;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Adapters.V8;
using WB.Core.SharedKernels.DataCollection.V2;
using WB.Core.SharedKernels.DataCollection.V4;
using WB.Core.SharedKernels.DataCollection.V5;
using WB.Core.SharedKernels.DataCollection.V6;
using WB.Core.SharedKernels.DataCollection.V7;
using WB.Core.SharedKernels.DataCollection.V8;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    public class InterviewExpressionStateUpgrader : IInterviewExpressionStateUpgrader
    {
        public ILatestInterviewExpressionState UpgradeToLatestVersionIfNeeded(IInterviewExpressionState state) =>
            InterviewExpressionStateV8ToLatestAdapter.AdaptIfNeeded(
            InterviewExpressionStateV7ToV8Adapter.AdaptIfNeeded(
            InterviewExpressionStateV6ToV7Adapter.AdaptIfNeeded(
            InterviewExpressionStateV5ToV6Adapter.AdaptIfNeeded(
            InterviewExpressionStateV4ToV5Adapter.AdaptIfNeeded(
            InterviewExpressionStateV2ToV4Adapter.AdaptIfNeeded(
            InterviewExpressionStateV1ToV2Adapter.AdaptIfNeeded(state)))))));
    }
}
