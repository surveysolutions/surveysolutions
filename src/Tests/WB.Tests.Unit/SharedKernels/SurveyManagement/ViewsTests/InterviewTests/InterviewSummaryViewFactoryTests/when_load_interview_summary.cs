using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Portable;
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


        private static InterviewSummaryViewFactory factory;

        private static Guid interviewId = Guid.Parse("11111111111111111111111111111111");
        private static string interviewerName = "interviewer";
        private static string supervisorName = "supervisor";
        private static InterviewSummary viewModel;
    }
}
