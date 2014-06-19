using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Schedulers;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Schedulers.InterviewDetailsDataScheduler;
using WB.Core.SharedKernels.SurveyManagement.Tests.SynchronizationTests.SchedulersTests.InterviewDetailsDataSchedulerTests;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.SynchronizationTests.SchedulersTests.InterviewDetailsDataProcessorTests
{
    internal class when_process_and_no_new_capi_packages : InterviewDetailsDataSchedulerTestsContext
    {
        Establish context = () =>
        {
            var interviewDetailsDataProcessorContextMock =
                new Mock<InterviewDetailsDataProcessorContext>(Mock.Of<IPlainStorageAccessor<SynchronizationStatus>>());
            interviewDetailsDataProcessorContextMock.Setup(_ => _.PushMessage(Moq.It.IsAny<string>()))
                .Callback((string message) => lastContextMessage = message);

            interviewDetailsDataProcessorContext = interviewDetailsDataProcessorContextMock.Object;

            interviewDetailsDataProcessor = Create.InterviewDetailsDataProcessor(interviewDetailsDataProcessorContext);
        };

        Because of = () =>
            interviewDetailsDataProcessor.Process();

        It should_messages_of_interview_details_data_processor_context_not_be_empty = () =>
            lastContextMessage.ShouldNotBeNull();

        It should_last_message_of_interview_details_data_processor_context_contains___packages_was_not_found = () =>
            lastContextMessage.ShouldContain("packages was not found");

        private static InterviewDetailsDataProcessorContext interviewDetailsDataProcessorContext;
        private static IInterviewDetailsDataProcessor interviewDetailsDataProcessor;
        private static string lastContextMessage;
    }
}