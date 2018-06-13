using System;
using FluentAssertions;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;


namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ViewsTests.InterviewTests.InterviewSummaryViewFactoryTests
{
    internal class when_load_interview_summary_and_summary_does_not_exists : InterviewSummaryViewFactoryTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var interviewSummaryReaderMock = new Mock<IReadSideRepositoryReader<InterviewSummary>>();
            interviewSummaryReaderMock.Setup(_ => _.GetById(interviewId.FormatGuid())).Returns(() => null);
            factory = CreateFactory(interviewSummaryReader: interviewSummaryReaderMock.Object);
            BecauseOf();
        }

        public void BecauseOf() => viewModel = factory.Load(interviewId);

        [NUnit.Framework.Test] public void should_view_model_be_null () =>
            viewModel.Should().BeNull();

        private static InterviewSummaryViewFactory factory;

        private static Guid interviewId = Guid.Parse("11111111111111111111111111111111");

        private static InterviewSummary viewModel;
    }
}
