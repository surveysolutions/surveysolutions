using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ViewsTests.InterviewTests.InterviewSummaryViewFactoryTests
{
    internal class when_load_interview_summary : InterviewSummaryViewFactoryTestsContext
    {
        Establish context = () =>
        {
            var interviewSummaryReaderMock = new Mock<IReadSideRepositoryReader<InterviewSummary>>();
            var interviewSummary = new InterviewSummary()
            {
                ResponsibleName = interviewerName,
                TeamLeadName = supervisorName
            };
            interviewSummary.CommentedStatusesHistory.Add(new InterviewCommentedStatus {
                Status = lastStatus,
                Date = lastStatusDateTime
            });

            interviewSummaryReaderMock.Setup(_ => _.GetById(interviewId.FormatGuid()))
                .Returns(interviewSummary);

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
            viewModel.CommentedStatusesHistory.First().Status.ShouldEqual(lastStatus);

        It should_date_from_first_element_of_CommentedStatusesHistory_be_equal_to_lastStatusDateTime = () =>
            viewModel.CommentedStatusesHistory.First().Date.ShouldEqual(lastStatusDateTime);


        private static InterviewSummaryViewFactory factory;

        private static Guid interviewId = Guid.Parse("11111111111111111111111111111111");
        private static string interviewerName = "interviewer";
        private static string supervisorName = "supervisor";
        private static InterviewStatus lastStatus = InterviewStatus.Completed;
        private static DateTime lastStatusDateTime = DateTime.Parse("2/2/2");

        private static InterviewSummary viewModel;
    }
}
