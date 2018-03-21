using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;


namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ViewsTests.InterviewTests.InterviewSummaryViewFactoryTests
{
    internal class when_load_interview_summary : InterviewSummaryViewFactoryTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var interviewSummaryReaderMock = new Mock<IReadSideRepositoryReader<InterviewSummary>>();
            var interviewSummary = new InterviewSummary()
            {
                ResponsibleName = interviewerName,
                TeamLeadName = supervisorName
            };

            interviewSummaryReaderMock.Setup(_ => _.GetById(interviewId.FormatGuid()))
                .Returns(interviewSummary);

            factory = CreateFactory(interviewSummaryReader: interviewSummaryReaderMock.Object);
            BecauseOf();
        }

        public void BecauseOf() => viewModel = factory.Load(interviewId);

        [NUnit.Framework.Test] public void should_view_model_not_be_null () =>
            viewModel.Should().NotBeNull();

        [NUnit.Framework.Test] public void should_ResponsibleName_be_equal_to_interviewerName () =>
            viewModel.ResponsibleName.Should().Be(interviewerName);

        [NUnit.Framework.Test] public void should_TeamLeadName_be_equal_to_supervisorName () =>
            viewModel.TeamLeadName.Should().Be(supervisorName);


        private static InterviewSummaryViewFactory factory;

        private static Guid interviewId = Guid.Parse("11111111111111111111111111111111");
        private static string interviewerName = "interviewer";
        private static string supervisorName = "supervisor";
        private static InterviewSummary viewModel;
    }
}
