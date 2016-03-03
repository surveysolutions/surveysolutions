using System;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Entities.Interview;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.FilteredSingleOptionQuestionViewModelTests
{
    internal class when_setting_text_existing_option : FilteredSingleOptionQuestionViewModelTestsContext
    {
        Establish context = () =>
        {
            interviewId = "interviewId";
            questionnaireId = "questionnaireId";
            userId = Guid.NewGuid();

            var singleOptionAnswer = Mock.Of<SingleOptionAnswer>(_ => _.Answer == 3);

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

            var navigationState = Create.NavigationState();
            viewModel.Init(interviewId, questionIdentity, navigationState);
        };

        Because of = () =>
            viewModel.FilterText = "é";

        It should_set_value = () =>
            viewModel.FilterText.ShouldEqual("é");

        It should_provide_suggesions = () =>
            viewModel.AutoCompleteSuggestions.Count.ShouldEqual(1);
        

        private static FilteredSingleOptionQuestionViewModel viewModel;
        private static Mock<QuestionStateViewModel<SingleOptionQuestionAnswered>> questionStateMock;
        private static Mock<AnsweringViewModel> answeringViewModelMock;
        private static string interviewId;
        private static string questionnaireId;
        private static Guid userId;
    }
}