using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.QuestionHeaderViewModelTests
{
    [Subject(typeof(QuestionHeaderViewModel))]
    internal class QuestionHeaderViewModelTestsContext
    {
        public static QuestionHeaderViewModel CreateViewModel(IQuestionnaireStorage questionnaireRepository = null, 
            IStatefulInterviewRepository interviewRepository = null, 
            ILiteEventRegistry registry = null)
        {
            var statefulInterviewRepository = interviewRepository ?? Mock.Of<IStatefulInterviewRepository>();
            var liteEventRegistry = registry ?? Create.Service.LiteEventRegistry();
            var questionnaireStorage = questionnaireRepository ?? Mock.Of<IQuestionnaireStorage>();

            return new QuestionHeaderViewModel(
                Create.ViewModel.DynamicTextViewModel(
                    interviewRepository: interviewRepository,
                    eventRegistry: liteEventRegistry),
                new EnablementViewModel(statefulInterviewRepository, liteEventRegistry, questionnaireStorage));
        }
    }
}