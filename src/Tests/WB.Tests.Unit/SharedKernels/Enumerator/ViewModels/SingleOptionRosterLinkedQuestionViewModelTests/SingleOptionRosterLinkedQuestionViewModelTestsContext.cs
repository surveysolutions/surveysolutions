using Machine.Specifications;
using NSubstitute;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.SingleOptionRosterLinkedQuestionViewModelTests
{
    [Subject(typeof(SingleOptionRosterLinkedQuestionViewModel))]
    internal class SingleOptionRosterLinkedQuestionViewModelTestsContext
    {
        protected static SingleOptionRosterLinkedQuestionViewModel CreateViewModel(IStatefulInterviewRepository interviewRepository = null, 
            IQuestionnaireStorage questionnaireRepository = null)
        {
            return new SingleOptionRosterLinkedQuestionViewModel(Substitute.For<IPrincipal>(),
                questionnaireRepository ?? Substitute.For<IQuestionnaireStorage>(),
                interviewRepository ?? Substitute.For<IStatefulInterviewRepository>(),
                Create.Service.LiteEventRegistry(),
                Stub.MvxMainThreadDispatcher(),
                Stub<QuestionStateViewModel<SingleOptionLinkedQuestionAnswered>>.WithNotEmptyValues,
                Substitute.For<QuestionInstructionViewModel>(),
                Substitute.For<AnsweringViewModel>()
                );
        }
    }
}