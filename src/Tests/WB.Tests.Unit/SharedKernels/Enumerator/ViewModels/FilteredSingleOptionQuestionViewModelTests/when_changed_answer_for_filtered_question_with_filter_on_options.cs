using System;
using System.Collections.Generic;
using Machine.Specifications;
using Moq;
using Rhino.Mocks;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Entities.Interview;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.FilteredSingleOptionQuestionViewModelTests
{
    internal class when_changed_answer_for_filtered_question_with_filter_on_options : FilteredSingleOptionQuestionViewModelTestsContext
    {
        Establish context = () =>
        {
            var interviewId = "interviewId";
            var singleOptionAnswer = Mock.Of<SingleOptionAnswer>(_ => _.Answer == 3);

            var interview = Mock.Of<IStatefulInterview>(_ => _.QuestionnaireIdentity == questionnaireId
                  && _.GetSingleOptionAnswer(questionIdentity) == singleOptionAnswer);

            var interviewRepository = Mock.Of<IStatefulInterviewRepository>(_ => _.Get(interviewId) == interview);
            var questionStateMock = new Mock<QuestionStateViewModel<SingleOptionQuestionAnswered>> { DefaultValue = DefaultValue.Mock };
            var answeringViewModelMock = new Mock<AnsweringViewModel>() { DefaultValue = DefaultValue.Mock };
            filteredOptionsViewModel = new Mock<FilteredOptionsViewModel>();

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