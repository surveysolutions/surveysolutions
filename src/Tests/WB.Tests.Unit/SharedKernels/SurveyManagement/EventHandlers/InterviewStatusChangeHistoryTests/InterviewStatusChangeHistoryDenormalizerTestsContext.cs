using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.InterviewStatusChangeHistoryTests
{
    internal class InterviewStatusChangeHistoryDenormalizerTestsContext
    {
        public static StatusChangeHistoryDenormalizerFunctional CreateDenormalizer()
        {
            return new StatusChangeHistoryDenormalizerFunctional(null, null);
        }
    }
}