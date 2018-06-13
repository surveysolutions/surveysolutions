using System;
using FluentAssertions;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Controllers;


namespace WB.Tests.Unit.SharedKernels.SurveyManagement.InterviewApiControllerTests
{
    internal class when_getting_interview_summary_for_map_point_and_interview_summary_does_not_contains_history_of_statuses : InterviewApiControllerTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var interviewSummaryViewFactoryMock = new Mock<IInterviewSummaryViewFactory>();
            interviewSummaryViewFactoryMock.Setup(_ => _.Load(interviewId)).Returns(new InterviewSummary()
            {
                ResponsibleName = interviewerName,
                TeamLeadName = supervisorName
            });

            controller = CreateController(interviewSummaryViewFactory: interviewSummaryViewFactoryMock.Object);
            BecauseOf();
        }

        public void BecauseOf() =>
            viewModel =
                controller.InterviewSummaryForMapPoint(new InterviewSummaryForMapPointViewModel(){InterviewId = interviewId});

        [NUnit.Framework.Test] public void should_view_model_not_be_null () =>
            viewModel.Should().NotBeNull();

        [NUnit.Framework.Test] public void should_interviewer_name_be_equal_to_interviewerName () =>
            viewModel.InterviewerName.Should().Be(interviewerName);

        [NUnit.Framework.Test] public void should_supervisor_name_be_equal_to_supervisorName () =>
            viewModel.SupervisorName.Should().Be(supervisorName);

        private static InterviewApiController controller;
        private static InterviewSummaryForMapPointView viewModel;
        private static Guid interviewId = Guid.Parse("11111111111111111111111111111111");
        private static string interviewerName = "interviewer";
        private static string supervisorName = "supervisor";
    }
}
