using System;
using System.Collections.Generic;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.ViewsTests.InterviewTests.InterviewSummaryViewFactoryTests
{
    internal class when_load_interview_summary : InterviewSummaryViewFactoryTestsContext
    {
        Establish context = () =>
        {
            var interviewSummaryReaderMock = new Mock<IReadSideRepositoryReader<InterviewSummary>>();
            interviewSummaryReaderMock.Setup(_ => _.GetById(interviewId)).Returns(new InterviewSummary()
            {
                ResponsibleName = interviewerName,
                TeamLeadName = supervisorName,
                CommentedStatusesHistory =
                    new List<InterviewCommentedStatus>()
                    {
                        new InterviewCommentedStatus() {Status = lastStatus, Date = lastStatusDateTime}
                    }
            });
            factory = CreateFactory(interviewSummaryReader: interviewSummaryReaderMock.Object);
        };

        Because of = () => viewModel = factory.Load(interviewId);

        It should_view_model_not_be_null = () =>
            viewModel.ShouldNotBeNull();

        It should_ResponsibleName_be_equal_to_interviewerName = () =>
            viewModel.ResponsibleName.ShouldEqual(interviewerName);

        It should_TeamLeadName_be_equal_to_supervisorName = () =>
            viewModel.TeamLeadName.ShouldEqual(supervisorName);

        It should_CommentedStatusesHistory_not_be_null = () =>
            viewModel.CommentedStatusesHistory.ShouldNotBeNull();

        It should_CommentedStatusesHistory_not_be_empty = () =>
            viewModel.CommentedStatusesHistory.ShouldNotBeEmpty();

        It should_status_from_first_element_of_CommentedStatusesHistory_be_equal_to_lastStatus = () =>
            viewModel.CommentedStatusesHistory[0].Status.ShouldEqual(lastStatus);

        It should_date_from_first_element_of_CommentedStatusesHistory_be_equal_to_lastStatusDateTime = () =>
            viewModel.CommentedStatusesHistory[0].Date.ShouldEqual(lastStatusDateTime);


        private static InterviewSummaryViewFactory factory;

        private static string interviewId = "11111111111111111111111111111111";
        private static string interviewerName = "interviewer";
        private static string supervisorName = "supervisor";
        private static InterviewStatus lastStatus = InterviewStatus.Completed;
        private static DateTime lastStatusDateTime = DateTime.Parse("2/2/2");

        private static InterviewSummary viewModel;
    }
}
