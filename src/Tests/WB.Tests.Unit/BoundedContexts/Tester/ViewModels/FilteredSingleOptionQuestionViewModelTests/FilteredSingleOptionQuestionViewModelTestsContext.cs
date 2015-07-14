using Chance.MvvmCross.Plugins.UserInteraction;
using Moq;
using WB.Core.BoundedContexts.Tester.Implementation.Entities;
using WB.Core.BoundedContexts.Tester.Infrastructure;
using WB.Core.BoundedContexts.Tester.Repositories;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.Core.BoundedContexts.Tester.ViewModels.Questions.State;
using WB.Core.BoundedContexts.Tester.ViewModels.Questions;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Unit.BoundedContexts.QuestionnaireTester.ViewModels.FilteredSingleOptionQuestionViewModelTests
{
    public class FilteredSingleOptionQuestionViewModelTestsContext
    {
        protected static FilteredSingleOptionQuestionViewModel CreateFilteredSingleOptionQuestionViewModel(
            QuestionStateViewModel<SingleOptionQuestionAnswered> questionStateViewModel,
            AnsweringViewModel answering,
            IPrincipal principal = null,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository = null,
            IStatefulInterviewRepository interviewRepository = null)
        {
            return new FilteredSingleOptionQuestionViewModel(
                principal ?? Mock.Of<IPrincipal>(),
                questionnaireRepository ?? Mock.Of<IPlainKeyValueStorage<QuestionnaireModel>>(),
                interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                questionStateViewModel ?? Mock.Of<QuestionStateViewModel<SingleOptionQuestionAnswered>>(),
                answering ?? Mock.Of<AnsweringViewModel>());
        }
    }
}