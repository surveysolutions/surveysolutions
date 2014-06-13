using System;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Schedulers.InterviewDetailsDataScheduler;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.SynchronizationTests.SchedulersTests.InterviewDetailsDataSchedulerTests
{
    internal class when_loading_interview_details_from_capi_package : InterviewDetailsDataSchedulerTestsContext
    {
        Establish context = () =>
        {
            interviewDetailsDataLoader = Create.InterviewDetailsDataLoader(interviewDetailsDataProcessorMock.Object);
        };

        Because of = () =>
            interviewDetailsDataLoader.Load();

        It should_process_via_interview_details_data_processor = () =>
            interviewDetailsDataProcessorMock.Verify(interviewDetailsDataProcessor => interviewDetailsDataProcessor.Process(), Times.Once);

        private static Mock<IInterviewDetailsDataProcessor> interviewDetailsDataProcessorMock = new Mock<IInterviewDetailsDataProcessor>();
        private static IInterviewDetailsDataLoader interviewDetailsDataLoader;
    }
}