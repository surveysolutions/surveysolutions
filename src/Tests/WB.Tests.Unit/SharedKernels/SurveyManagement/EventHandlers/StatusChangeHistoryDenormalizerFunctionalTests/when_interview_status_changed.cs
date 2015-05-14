using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.StatusChangeHistoryDenormalizerFunctionalTests
{
    internal class when_interview_status_changed : StatusChangeHistoryDenormalizerFunctionalTestContext
    {
        private Establish context = () =>
        {
            history = new InterviewStatuses();
            history.InterviewCommentedStatuses.Add(new InterviewCommentedStatus {Status = InterviewStatus.InterviewerAssigned});
            denormalizer = CreateDenormalizer();
        };

        private Because of =
            () =>
                history =
                    denormalizer.Update(history,
                        Create.InterviewCompletedEvent(comment: "comment"));

        private It should_put_comment_to_last_status =
            () => history.InterviewCommentedStatuses.Last().Comment.ShouldEqual("comment");

        private static StatusChangeHistoryDenormalizerFunctional denormalizer;
        private static InterviewStatuses history;

    }
}