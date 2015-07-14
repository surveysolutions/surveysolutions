using Chance.MvvmCross.Plugins.UserInteraction;

using Moq;

using WB.Core.BoundedContexts.Tester.Implementation.Aggregates;
using WB.Core.BoundedContexts.Tester.Implementation.Entities;
using WB.Core.BoundedContexts.Tester.Infrastructure;
using WB.Core.BoundedContexts.Tester.Repositories;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.Core.BoundedContexts.Tester.ViewModels.Questions.State;
using WB.Core.BoundedContexts.Tester.ViewModels.Questions;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;

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
            IStatefulInterviewRepository interviewRepository = null,
            
            IUserInteraction userInteraction = null)
        {
            return new TextListQuestionViewModel(
                principal ?? Mock.Of<IPrincipal>(),
                questionnaireRepository ?? Mock.Of<IPlainKeyValueStorage<QuestionnaireModel>>(),
                interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                questionStateViewModel ?? Mock.Of<QuestionStateViewModel<TextListQuestionAnswered>>(),
                userInteraction ?? Mock.Of<IUserInteraction>(),
                answering ?? Mock.Of<AnsweringViewModel>());
        }
    }
}
