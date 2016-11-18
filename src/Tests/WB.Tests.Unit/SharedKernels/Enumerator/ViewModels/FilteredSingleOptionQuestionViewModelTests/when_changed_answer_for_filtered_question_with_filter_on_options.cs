using System;
using System.Linq;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.Enumerator.Aggregates;

using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.FilteredSingleOptionQuestionViewModelTests
{
    internal class when_changed_answer_for_filtered_question_with_filter_on_options : FilteredSingleOptionQuestionViewModelTestsContext
    {
        private Establish context = () =>
        {
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
            var answeringViewModelMock = new Mock<AnsweringViewModel>() { DefaultValue = DefaultValue.Mock };
            filteredOptionsViewModel = new Mock<FilteredOptionsViewModel>();
            filteredOptionsViewModel.Setup(x => x.GetOptions(Moq.It.IsAny<string>())).Returns((string filter) => 
                interview.Object.GetTopFilteredOptionsForQuestion(questionIdentity, null, filter, 15));

            viewModel = CreateFilteredSingleOptionQuestionViewModel(
                questionStateViewModel: questionStateMock.Object,
                answering: answeringViewModelMock.Object,
                interviewRepository: interviewRepository,
                filteredOptionsViewModel: filteredOptionsViewModel.Object);

            var navigationState = Create.Other.NavigationState();
            viewModel.Init(interviewId, questionIdentity, navigationState);

            filteredOptionsViewModel.ResetCalls();
        };

        Because of = () =>
            filteredOptionsViewModel.Raise(_ => _.OptionsChanged -= null, EventArgs.Empty);

        It should_update_suggestions_list = () =>
            filteredOptionsViewModel.Verify(_ => _.GetOptions(Moq.It.IsAny<string>()), Times.Once);


        private static FilteredSingleOptionQuestionViewModel viewModel;
        private static Mock<FilteredOptionsViewModel> filteredOptionsViewModel;
    }
}