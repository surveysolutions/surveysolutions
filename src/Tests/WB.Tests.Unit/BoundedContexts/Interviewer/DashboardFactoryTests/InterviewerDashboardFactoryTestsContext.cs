using Moq;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.DashboardFactoryTests
{
    internal class InterviewerDashboardFactoryTestsContext
    {
        public static InterviewerDashboardFactory CreateInterviewerDashboardFactory(IPlainStorage<InterviewView> interviewViewRepository = null,
            IPlainStorage<QuestionnaireView> questionnaireViewRepository = null,
            IInterviewViewModelFactory interviewViewModelFactory = null)
        {
            return new InterviewerDashboardFactory(
                interviewViewRepository: interviewViewRepository ?? Mock.Of<IPlainStorage<InterviewView>>(),
                questionnaireViewRepository: questionnaireViewRepository ?? Mock.Of<IPlainStorage<QuestionnaireView>>(),
                interviewViewModelFactory: interviewViewModelFactory ?? Mock.Of<IInterviewViewModelFactory>());
        }
    }
}
