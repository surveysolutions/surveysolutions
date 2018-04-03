using System;
using FluentAssertions;
using Moq;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Controllers;


namespace WB.Tests.Unit.SharedKernels.SurveyManagement.InterviewApiControllerTests
{
    internal class when_getting_interview_summary_for_map_point : InterviewApiControllerTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var interviewSummaryViewFactoryMock = new Mock<IInterviewSummaryViewFactory>();
            var interviewSummary = new InterviewSummary()
            {
                ResponsibleName = interviewerName,
                TeamLeadName = supervisorName
            };
            interviewSummary.Status = lastStatus;
            interviewSummary.UpdateDate =  lastStatusDateTime;

            interviewSummaryViewFactoryMock.Setup(_ => _.Load(interviewId)).Returns(interviewSummary);

            controller = CreateController(interviewSummaryViewFactory: interviewSummaryViewFactoryMock.Object);
            BecauseOf();
        }

        public void BecauseOf() =>
            viewModel =
                controller.InterviewSummaryForMapPoint(new InterviewSummaryForMapPointViewModel() { InterviewId = interviewId });

        [NUnit.Framework.Test] public void should_view_model_not_be_null () =>
            viewModel.Should().NotBeNull();

        [NUnit.Framework.Test] public void should_interviewer_name_be_equal_to_interviewerName () =>
            viewModel.InterviewerName.Should().Be(interviewerName);

        [NUnit.Framework.Test] public void should_supervisor_name_be_equal_to_supervisorName () =>
            viewModel.SupervisorName.Should().Be(supervisorName);

        [NUnit.Framework.Test] public void should_last_status_be_equal_to_lastStatus () =>
            viewModel.LastStatus.Should().Be(lastStatus.ToLocalizeString());

        [NUnit.Framework.Test] public void should_last_status_date_be_equal_to_lastStatusDate () =>
            viewModel.LastUpdatedDate.Should().Be(AnswerUtils.AnswerToString(lastStatusDateTime));

        private static InterviewApiController controller;
        private static InterviewSummaryForMapPointView viewModel;
        private static Guid interviewId = Guid.Parse("11111111111111111111111111111111");
        private static string interviewerName = "interviewer";
        private static string supervisorName = "supervisor";
        private static InterviewStatus lastStatus = InterviewStatus.Completed;
        private static DateTime lastStatusDateTime = DateTime.Parse("2/2/2");
    }
}
