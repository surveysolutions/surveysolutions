using Chance.MvvmCross.Plugins.UserInteraction;

using Moq;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionStateViewModels;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

using it = Moq.It;

namespace WB.Tests.Unit.BoundedContexts.Tester.TextListQuestionViewModelTests
{
    public class TextListQuestionViewModelTestContext
    {
        protected static TextListQuestionViewModel CreateTextListQuestionViewModel(
            QuestionStateViewModel<TextListQuestionAnswered> questionStateViewModel,
            AnsweringViewModel answering,
            IPrincipal principal = null,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository = null,
            IStatefullInterviewRepository interviewRepository = null,
            
            IUserInteraction userInteraction = null)
        {
            return new TextListQuestionViewModel(
                principal ?? Mock.Of<IPrincipal>(),
                questionnaireRepository ?? Mock.Of<IPlainKeyValueStorage<QuestionnaireModel>>(),
                interviewRepository ?? Mock.Of<IStatefullInterviewRepository>(),
                questionStateViewModel ?? Mock.Of<QuestionStateViewModel<TextListQuestionAnswered>>(),
                userInteraction ?? Mock.Of<IUserInteraction>(),
                answering ?? Mock.Of<AnsweringViewModel>());
        }
    }
}
