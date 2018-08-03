using System;
using System.Linq;
using System.Threading;
using Moq;
using MvvmCross.Plugin.Messenger;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.FilteredSingleOptionQuestionViewModelTests
{
    internal class when_changed_answer_for_filtered_question_with_filter_on_options : FilteredSingleOptionQuestionViewModelTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var interviewId = "interviewId";
            var singleOptionAnswer = Mock.Of<InterviewTreeSingleOptionQuestion>(_ => _.GetAnswer() == Create.Entity.SingleOptionAnswer(3));
            
            var interview = new Mock<IStatefulInterview>();

            interview.Setup(x => x.QuestionnaireIdentity).Returns(questionnaireId);
            interview.Setup(x => x.GetSingleOptionQuestion(questionIdentity)).Returns(singleOptionAnswer);
            interview.Setup(x => x.GetOptionForQuestionWithoutFilter(questionIdentity, 3, 1)).Returns(new CategoricalOption() { Title = "3", Value = 3, ParentValue = null });
            interview.Setup(x => x.GetTopFilteredOptionsForQuestion(Moq.It.IsAny<Identity>(), Moq.It.IsAny<int?>(), Moq.It.IsAny<string>(), Moq.It.IsAny<int>()))
                .Returns((Identity identity, int? value, string filter, int count) => Options.Where(x => x.ParentValue == value &&(filter == null || x.Title.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)).ToList());


            var interviewRepository = Mock.Of<IStatefulInterviewRepository>(_ => _.Get(interviewId) == interview.Object);
            var questionStateMock = new Mock<QuestionStateViewModel<SingleOptionQuestionAnswered>> { DefaultValue = DefaultValue.Mock };

            var answerViewModel = new AnsweringViewModel(Mock.Of<ICommandService>(), Mock.Of<IUserInterfaceStateService>(), Mock.Of<IMvxMessenger>());

            filteredOptionsViewModel = new Mock<FilteredOptionsViewModel>();
            filteredOptionsViewModel.Setup(x => x.GetOptions(Moq.It.IsAny<string>())).Returns((string filter) => 
                interview.Object.GetTopFilteredOptionsForQuestion(questionIdentity, null, filter, 15));

            viewModel = CreateFilteredSingleOptionQuestionViewModel(
                questionStateViewModel: questionStateMock.Object,
                answering: answerViewModel,
                interviewRepository: interviewRepository,
                filteredOptionsViewModel: filteredOptionsViewModel.Object);
            
            var navigationState = Create.Other.NavigationState();
            viewModel.Init(interviewId, questionIdentity, navigationState);
            Thread.Sleep(1000);
            filteredOptionsViewModel.ResetCalls();
            BecauseOf();
        }

        public void BecauseOf() {
            filteredOptionsViewModel.Raise(_ => _.OptionsChanged -= null, EventArgs.Empty);
            Thread.Sleep(1000);
        }

        [NUnit.Framework.Test] public void should_update_suggestions_list () =>
            filteredOptionsViewModel.Verify(_ => _.GetOptions(Moq.It.IsAny<string>()), Times.Once);

        private static FilteredSingleOptionQuestionViewModel viewModel;
        private static Mock<FilteredOptionsViewModel> filteredOptionsViewModel;
    }
}
