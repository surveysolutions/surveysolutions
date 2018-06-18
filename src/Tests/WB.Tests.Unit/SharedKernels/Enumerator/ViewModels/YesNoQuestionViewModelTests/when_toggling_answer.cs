using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.YesNoQuestionViewModelTests
{
    internal class when_toggling_answer : YesNoQuestionViewModelTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public async Task context () {
            questionGuid = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            questionId = Create.Entity.Identity(questionGuid, Empty.RosterVector);

            var questionnaire = Mock.Of<IQuestionnaire>(_
                => _.ShouldQuestionRecordAnswersOrder(questionId.Id) == false
                && _.GetMaxSelectedAnswerOptions(questionId.Id) == null
                && _.IsRosterSizeQuestion(questionId.Id) == false
            );

            var filteredOptionsViewModel = Setup.FilteredOptionsViewModel(new List<CategoricalOption>
            {
                Create.Entity.CategoricalQuestionOption(1, "item1"),
                Create.Entity.CategoricalQuestionOption(2, "item2"),
                Create.Entity.CategoricalQuestionOption(3, "item3"),
                Create.Entity.CategoricalQuestionOption(4, "item4"),
                Create.Entity.CategoricalQuestionOption(5, "item5"),
            });

            var yesNoAnswer = Create.Entity.InterviewTreeYesNoQuestion(new[]
            {
                new AnsweredYesNoOption(5, true),
                new AnsweredYesNoOption(2, false),
            });

            var interview = Mock.Of<IStatefulInterview>(x => x.GetYesNoQuestion(questionId) == yesNoAnswer);

            var questionnaireStorage = new Mock<IQuestionnaireStorage>();
            var interviewRepository = new Mock<IStatefulInterviewRepository>();

            questionnaireStorage.SetReturnsDefault(questionnaire);
            interviewRepository.SetReturnsDefault(interview);

            answeringMock = new Mock<AnsweringViewModel>();

            viewModel = CreateViewModel(questionnaireStorage: questionnaireStorage.Object,
                interviewRepository: interviewRepository.Object,
                answeringViewModel: answeringMock.Object,
                filteredOptionsViewModel: filteredOptionsViewModel);

            viewModel.Init("blah", questionId, Create.Other.NavigationState());
            await BecauseOf();
        }

        public async Task BecauseOf()
        {
            Thread.Sleep(1);
            viewModel.Options.Second().YesSelected = true;
            await viewModel.ToggleAnswerAsync(viewModel.Options.Second());
        }

        [NUnit.Framework.Test] public void should_send_command_to_service () => answeringMock.Verify(x => x.SendAnswerQuestionCommandAsync(Moq.It.Is<AnswerYesNoQuestion>(c =>
            c.AnsweredOptions[0].Yes && c.AnsweredOptions[0].OptionValue == 5 &&
            c.AnsweredOptions[1].Yes && c.AnsweredOptions[1].OptionValue == 2
        )));

        static YesNoQuestionViewModel viewModel;
        static Identity questionId;
        static Guid questionGuid;
        static Mock<AnsweringViewModel> answeringMock;
    }
}

