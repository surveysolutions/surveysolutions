using Moq;
using MvvmCross.Test.Core;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.TextListQuestionViewModelTests
{
    internal class TextListQuestionViewModelTestContext : MvxIoCSupportingTest
    {
        public TextListQuestionViewModelTestContext()
        {
            base.Setup();
        }
        protected static TextListQuestionViewModel CreateTextListQuestionViewModel(
            QuestionStateViewModel<TextListQuestionAnswered> questionStateViewModel,
            AnsweringViewModel answering,
            IPrincipal principal = null,
            IPlainQuestionnaireRepository questionnaireRepository = null,
            IStatefulInterviewRepository interviewRepository = null,
            
            IUserInteractionService userInteractionService = null)
        {
            return new TextListQuestionViewModel(
                principal ?? Mock.Of<IPrincipal>(),
                questionnaireRepository ?? Mock.Of<IPlainQuestionnaireRepository>(),
                interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                questionStateViewModel ?? Mock.Of<QuestionStateViewModel<TextListQuestionAnswered>>(),
                userInteractionService ?? Mock.Of<IUserInteractionService>(),
                answering ?? Mock.Of<AnsweringViewModel>());
        }
    }
}
