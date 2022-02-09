using System;
using System.Linq;
using System.Threading;
using Moq;
using MvvmCross.Plugin.Messenger;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.FilteredSingleOptionQuestionViewModelTests
{
    internal class when_changed_answer_for_filtered_question_with_filter_on_options : FilteredSingleOptionQuestionViewModelTestsContext
    {
        [Test] 
        public void should_update_suggestions_list () {
            var interviewId = "interviewId";
            var singleOptionAnswer = Mock.Of<InterviewTreeSingleOptionQuestion>(_ => _.GetAnswer() == Create.Entity.SingleOptionAnswer(3));
            
            var interview = new Mock<IStatefulInterview>();

            interview.Setup(x => x.QuestionnaireIdentity).Returns(questionnaireId);
            interview.Setup(x => x.GetSingleOptionQuestion(questionIdentity)).Returns(singleOptionAnswer);
            interview.Setup(x => x.GetOptionForQuestionWithoutFilter(questionIdentity, 3, 1)).Returns(new CategoricalOption() { Title = "3", Value = 3, ParentValue = null });
            interview.Setup(x => x.GetTopFilteredOptionsForQuestion(Moq.It.IsAny<Identity>(), Moq.It.IsAny<int?>(), Moq.It.IsAny<string>(), Moq.It.IsAny<int>(), It.IsAny<int[]>()))
                .Returns((Identity identity, int? value, string filter, int count, int[] excludedOptions) => Options.Where(x => x.ParentValue == value &&(filter == null || x.Title.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)).ToList());


            var interviewRepository = Create.Storage.InterviewRepository(interview.Object);
            var questionStateMock = new Mock<QuestionStateViewModel<SingleOptionQuestionAnswered>> { DefaultValue = DefaultValue.Mock };

            var answerViewModel = new AnsweringViewModel(Mock.Of<ICommandService>(), Mock.Of<IUserInterfaceStateService>(),  Mock.Of<ILogger>());

            var filteredOptionsViewModel = new Mock<FilteredOptionsViewModel>();
            filteredOptionsViewModel.Setup(x => x.GetAnsweredOption(3)).Returns(new CategoricalOption() { Title = "3", Value = 3, ParentValue = null });
            filteredOptionsViewModel.Setup(x => x.GetOptions(Moq.It.IsAny<string>(), It.IsAny<int[]>(), It.IsAny<int?>())).Returns((string filter, int[] excludedOptions, int? count) => 
                interview.Object.GetTopFilteredOptionsForQuestion(questionIdentity, null, filter, 15, null));

            var viewModel = CreateFilteredSingleOptionQuestionViewModel(
                questionStateViewModel: questionStateMock.Object,
                answering: answerViewModel,
                interviewRepository: interviewRepository,
                filteredOptionsViewModel: filteredOptionsViewModel.Object);
            
            var navigationState = Create.Other.NavigationState();
            viewModel.Init(interviewId, questionIdentity, navigationState);
            Thread.Sleep(1000);
            filteredOptionsViewModel.Invocations.Clear();
            
            // Act
            filteredOptionsViewModel.Raise(_ => _.OptionsChanged -= null, EventArgs.Empty);
            Thread.Sleep(1000);
            
            // assert
            filteredOptionsViewModel.Verify(_ => _.GetOptions(Moq.It.IsAny<string>(), It.IsAny<int[]>(), It.IsAny<int?>()), Times.Once);
        }
    }
}
