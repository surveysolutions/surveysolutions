using NSubstitute;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.SingleOptionRosterLinkedQuestionViewModelTests
{
    [NUnit.Framework.TestOf(typeof(SingleOptionRosterLinkedQuestionViewModel))]
    internal class SingleOptionRosterLinkedQuestionViewModelTestsContext
    {
        protected static SingleOptionRosterLinkedQuestionViewModel CreateViewModel(IStatefulInterviewRepository interviewRepository = null, 
            IQuestionnaireStorage questionnaireRepository = null)
        {
            return new SingleOptionRosterLinkedQuestionViewModel(Substitute.For<IPrincipal>(),
                questionnaireRepository ?? Substitute.For<IQuestionnaireStorage>(),
                interviewRepository ?? Substitute.For<IStatefulInterviewRepository>(),
                Create.Service.LiteEventRegistry(),
                Stub.MvxMainThreadAsyncDispatcher(),
                Stub<QuestionStateViewModel<SingleOptionLinkedQuestionAnswered>>.WithNotEmptyValues,
                Substitute.For<QuestionInstructionViewModel>(),
                Substitute.For<AnsweringViewModel>()
                );
        }
    }
}
