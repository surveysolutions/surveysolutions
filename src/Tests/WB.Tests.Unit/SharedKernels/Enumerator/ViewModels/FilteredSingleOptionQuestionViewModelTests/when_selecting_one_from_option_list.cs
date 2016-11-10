using System;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.Enumerator.Aggregates;

using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.FilteredSingleOptionQuestionViewModelTests
{
    internal class when_selecting_one_from_option_list : FilteredSingleOptionQuestionViewModelTestsContext
    {
        Establish context = () =>
        {
            interviewId = "interviewId";
            userId = Guid.NewGuid();

            var singleOptionAnswer = Mock.Of<SingleOptionAnswer>(_ => _.Answer == 3);

            var interview = Mock.Of<IStatefulInterview>(_
                => _.QuestionnaireIdentity == questionnaireId
                   && _.GetSingleOptionQuestion(questionIdentity) == singleOptionAnswer);

            var interviewRepository = Mock.Of<IStatefulInterviewRepository>(_ => _.Get(interviewId) == interview);

            var userIdentity = Mock.Of<IUserIdentity>(_ => _.UserId == userId);
            var principal = Mock.Of<IPrincipal>(_ => _.CurrentUserIdentity == userIdentity);

            questionStateMock = new Mock<QuestionStateViewModel<SingleOptionQuestionAnswered>> { DefaultValue = DefaultValue.Mock };
            answeringViewModelMock = new Mock<AnsweringViewModel>() { DefaultValue = DefaultValue.Mock };

            var filteredOptionsViewModel = Setup.FilteredOptionsViewModel();

            viewModel = CreateFilteredSingleOptionQuestionViewModel(
                questionStateViewModel: questionStateMock.Object,
                answering: answeringViewModelMock.Object,
                principal: principal,
                interviewRepository: interviewRepository,
                filteredOptionsViewModel: filteredOptionsViewModel);

            var navigationState = Create.Other.NavigationState();
            
            viewModel.Init(interviewId, questionIdentity, navigationState);
        };

        Because of = () =>
            viewModel.SelectedObject = new FilteredSingleOptionQuestionViewModel.FilteredComboboxItemViewModel() { Text = "html", Value = 4 };

        It should_set_nonnull_answer = () =>
            viewModel.SelectedObject.ShouldNotBeNull();

        It should_set_to_answer_backend_value = () =>
            viewModel.SelectedObject.Value.ShouldEqual(4);


        private static FilteredSingleOptionQuestionViewModel viewModel;
        private static Mock<QuestionStateViewModel<SingleOptionQuestionAnswered>> questionStateMock;
        private static Mock<AnsweringViewModel> answeringViewModelMock;
        private static string interviewId;
        private static Guid userId;
    }
}