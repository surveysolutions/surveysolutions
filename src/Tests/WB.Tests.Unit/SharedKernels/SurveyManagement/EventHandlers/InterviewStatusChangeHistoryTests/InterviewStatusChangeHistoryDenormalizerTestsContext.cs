using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.InterviewStatusChangeHistoryTests
{
    internal class InterviewStatusChangeHistoryDenormalizerTestsContext
    {
        public static InterviewStatusChangeHistoryDenormalizer CreateDenormalizer()
        {
            return new InterviewStatusChangeHistoryDenormalizer(null, null);
        }
    }
}