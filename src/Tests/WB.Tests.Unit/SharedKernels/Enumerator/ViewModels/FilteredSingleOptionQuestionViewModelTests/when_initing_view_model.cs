using System;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Entities.Interview;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.FilteredSingleOptionQuestionViewModelTests
{
    internal class when_initing_view_model : FilteredSingleOptionQuestionViewModelTestsContext
    {
        Establish context = () =>
        {
            interviewId = "interviewId";
            questionnaireId = "questionnaireId";
            userId = Guid.NewGuid();

            var singleOptionAnswer = Mock.Of<SingleOptionAnswer>(_ => _.Answer == 3 && _.IsAnswered == true);

            var interview = Mock.Of<IStatefulInterview>(_
                => _.QuestionnaireId == questionnaireId
                   && _.GetSingleOptionAnswer(questionIdentity) == singleOptionAnswer);

            var interviewRepository = Mock.Of<IStatefulInterviewRepository>(_ => _.Get(interviewId) == interview);

            var questionnaireRepository = SetupQuestionnaireRepositoryWithFilteredQuestion();

            var userIdentity = Mock.Of<IUserIdentity>(_ => _.UserId == userId);
            var principal = Mock.Of<IPrincipal>(_ => _.CurrentUserIdentity == userIdentity);

            questionStateMock = new Mock<QuestionStateViewModel<SingleOptionQuestionAnswered>> { DefaultValue = DefaultValue.Mock };
            answeringViewModelMock = new Mock<AnsweringViewModel>() { DefaultValue = DefaultValue.Mock };
            
            viewModel = CreateFilteredSingleOptionQuestionViewModel(
                questionStateViewModel: questionStateMock.Object,
                answering: answeringViewModelMock.Object,
                principal: principal,
                interviewRepository: interviewRepository,
                questionnaireRepository: questionnaireRepository);

            navigationState = Create.NavigationState();
        };

        Because of = () =>
            viewModel.InitAsync(interviewId, questionIdentity, navigationState);

        It should_set_nonnull_answer = () =>
            viewModel.SelectedObject.ShouldNotBeNull();

        It should_set_to_answer_backend_value = () =>
            viewModel.SelectedObject.Value.ShouldEqual(3);

        It should_set_to_options_all_values = () =>
        {
            viewModel.Options.Count.ShouldEqual(5);
            viewModel.Options.ShouldContain(i => i.Value == 1);
            viewModel.Options.ShouldContain(i => i.Value == 2);
            viewModel.Options.ShouldContain(i => i.Value == 3);
            viewModel.Options.ShouldContain(i => i.Value == 4);
            viewModel.Options.ShouldContain(i => i.Value == 5);
        };

        private static FilteredSingleOptionQuestionViewModel viewModel;
        private static NavigationState navigationState;
        private static Mock<QuestionStateViewModel<SingleOptionQuestionAnswered>> questionStateMock;
        private static Mock<AnsweringViewModel> answeringViewModelMock;
        private static string interviewId;
        private static string questionnaireId;
        private static Guid userId;
    }
}