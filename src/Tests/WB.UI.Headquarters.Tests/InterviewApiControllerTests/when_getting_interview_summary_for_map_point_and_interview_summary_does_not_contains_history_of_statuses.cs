using System;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using It = Machine.Specifications.It;

namespace WB.UI.Headquarters.Tests.InterviewApiControllerTests
{
    internal class when_getting_interview_summary_for_map_point_and_interview_summary_does_not_contains_history_of_statuses : InterviewApiControllerTestsContext
    {
        private Establish context = () =>
        {
            var interviewSummaryViewFactoryMock = new Mock<IInterviewSummaryViewFactory>();
            interviewSummaryViewFactoryMock.Setup(_ => _.Load(interviewId)).Returns(new InterviewSummary()
            {
                ResponsibleName = interviewerName,
                TeamLeadName = supervisorName
            });

            controller = CreateController(interviewSummaryViewFactory: interviewSummaryViewFactoryMock.Object);
        };

        Because of = () =>
            viewModel =
                controller.InterviewSummaryForMapPoint(new InterviewSummaryForMapPointViewModel(){InterviewId = interviewId});

        It should_view_model_not_be_null = () =>
            viewModel.ShouldNotBeNull();

        It should_interviewer_name_be_equal_to_interviewerName = () =>
            viewModel.InterviewerName.ShouldEqual(interviewerName);

        It should_supervisor_name_be_equal_to_supervisorName = () =>
            viewModel.SupervisorName.ShouldEqual(supervisorName);

        It should_last_status_be_null = () =>
            viewModel.LastStatus.ShouldBeNull();

        It should_last_status_date_be_null = () =>
            viewModel.LastStatusChangeDate.ShouldBeNull();

        private static InterviewApiController controller;
        private static InterviewSummaryForMapPointView viewModel;
        private static Guid interviewId = Guid.Parse("11111111111111111111111111111111");
        private static string interviewerName = "interviewer";
        private static string supervisorName = "supervisor";
        private static InterviewStatus lastStatus = InterviewStatus.Completed;
        private static DateTime lastStatusDateTime = DateTime.Parse("2/2/2");
    }
}
