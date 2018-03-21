using System;
using System.Threading.Tasks;
using Moq;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.FilteredSingleOptionQuestionViewModelTests
{
    internal class when_selecting_one_from_option_list : FilteredSingleOptionQuestionViewModelTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public async Task context () {
            interviewId = "interviewId";
            userId = Guid.NewGuid();

            var singleOptionAnswer = Mock.Of<InterviewTreeSingleOptionQuestion>(_ => _.GetAnswer() == Create.Entity.SingleOptionAnswer(3));

            var interview = Mock.Of<IStatefulInterview>(_
                => _.QuestionnaireIdentity == questionnaireId
                   && _.GetSingleOptionQuestion(questionIdentity) == singleOptionAnswer
                   && _.GetOptionForQuestionWithFilter(questionIdentity, "html", null) == Create.Entity.CategoricalQuestionOption(4, "html", null));

            var interviewRepository = Mock.Of<IStatefulInterviewRepository>(_ => _.Get(interviewId) == interview);

            var userIdentity = Mock.Of<IUserIdentity>(_ => _.UserId == userId);
            var principal = Mock.Of<IPrincipal>(_ => _.CurrentUserIdentity == userIdentity);

            questionStateMock = new Mock<QuestionStateViewModel<SingleOptionQuestionAnswered>> { DefaultValue = DefaultValue.Mock };
            answeringViewModelMock = new Mock<AnsweringViewModel>() { DefaultValue = DefaultValue.Mock };

            var filteredOptionsViewModel = Abc.Setup.FilteredOptionsViewModel();

            viewModel = CreateFilteredSingleOptionQuestionViewModel(
                questionStateViewModel: questionStateMock.Object,
                answering: answeringViewModelMock.Object,
                principal: principal,
                interviewRepository: interviewRepository,
                filteredOptionsViewModel: filteredOptionsViewModel);

            var navigationState = Create.Other.NavigationState();
            
            viewModel.Init(interviewId, questionIdentity, navigationState);
            await BecauseOf();
        }

        public async Task BecauseOf() => await viewModel.SaveAnswerBySelectedOptionCommand.ExecuteAsync("html");

        [NUnit.Framework.Test] public void should_save_answer () =>
            answeringViewModelMock.Verify(x => x.SendAnswerQuestionCommandAsync(Moq.It.Is<AnswerSingleOptionQuestionCommand>(y=>y.SelectedValue == 4)), Times.Once);

        private static FilteredSingleOptionQuestionViewModel viewModel;
        private static Mock<QuestionStateViewModel<SingleOptionQuestionAnswered>> questionStateMock;
        private static Mock<AnsweringViewModel> answeringViewModelMock;
        private static string interviewId;
        private static Guid userId;
    }
}
