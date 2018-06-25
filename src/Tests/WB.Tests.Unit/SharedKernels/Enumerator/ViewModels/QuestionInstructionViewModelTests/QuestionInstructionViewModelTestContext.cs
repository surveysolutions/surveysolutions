using Moq;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.QuestionInstructionViewModelTests
{
    public class QuestionInstructionViewModelTestContext
    {
        public QuestionInstructionViewModel CreateQuestionHeaderViewModel(IQuestionnaireStorage questionnaireRepository = null,
            IStatefulInterviewRepository interviewRepository = null,
            ILiteEventRegistry registry = null)
        {
            var statefulInterviewRepository = interviewRepository ?? Mock.Of<IStatefulInterviewRepository>();
            var liteEventRegistry = registry ?? Create.Service.LiteEventRegistry();
            var questionnaireStorage = questionnaireRepository ?? Mock.Of<IQuestionnaireStorage>();

            return new QuestionInstructionViewModel(
                questionnaireStorage,
                statefulInterviewRepository,
                Create.ViewModel.DynamicTextViewModel(
                    interviewRepository: interviewRepository,
                    eventRegistry: liteEventRegistry));
        }
    }
}
