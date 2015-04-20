using System.Linq;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.InterviewStatusChangeHistoryTests
{
    internal class when_interview_status_changed : InterviewStatusChangeHistoryDenormalizerTestsContext
    {
        Establish context = () =>
        {
            history = new InterviewStatusHistory();
            history.StatusChangeHistory.Add(new InterviewCommentedStatus{Status = InterviewStatus.InterviewerAssigned});
            denormalizer = CreateDenormalizer();
        };

        Because of = () => history = denormalizer.Update(history, Create.InterviewStatusChangedEvent(InterviewStatus.InterviewerAssigned, comment: "comment"));

        It should_put_comment_to_last_status = () => history.StatusChangeHistory.Last().Comment.ShouldEqual("comment");

        static StatusChangeHistoryDenormalizerFunctional denormalizer;
        private static InterviewStatusHistory history;
    }
}

