using Cirrious.MvvmCross.Test.Core;

using Moq;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using it = Moq.It;

namespace WB.Tests.Unit.BoundedContexts.Tester.TextListQuestionViewModelTests
{
    public class TextListQuestionViewModelTestContext : MvxIoCSupportingTest
    {
        public TextListQuestionViewModelTestContext()
        {
            base.Setup();
        }
        protected static TextListQuestionViewModel CreateTextListQuestionViewModel(
            QuestionStateViewModel<TextListQuestionAnswered> questionStateViewModel,
            AnsweringViewModel answering,
            IPrincipal principal = null,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository = null,
            IStatefulInterviewRepository interviewRepository = null,
            
            IUserInteractionService userInteractionService = null)
        {
            return new TextListQuestionViewModel(
                principal ?? Mock.Of<IPrincipal>(),
                questionnaireRepository ?? Mock.Of<IPlainKeyValueStorage<QuestionnaireModel>>(),
                interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                questionStateViewModel ?? Mock.Of<QuestionStateViewModel<TextListQuestionAnswered>>(),
                userInteractionService ?? Mock.Of<IUserInteractionService>(),
                answering ?? Mock.Of<AnsweringViewModel>());
        }
    }
}
