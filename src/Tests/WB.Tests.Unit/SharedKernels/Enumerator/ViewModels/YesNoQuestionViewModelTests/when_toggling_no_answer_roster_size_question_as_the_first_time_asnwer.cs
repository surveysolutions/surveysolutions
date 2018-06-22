using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.YesNoQuestionViewModelTests
{
    internal class when_toggling_no_answer_roster_size_question_as_the_first_time_asnwer : YesNoQuestionViewModelTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            interviewIdAsString = "hello";
            questionGuid = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            questionId = Create.Entity.Identity(questionGuid, Empty.RosterVector);
           
            var questionnaire = Mock.Of<IQuestionnaire>(_ 
                => _.ShouldQuestionRecordAnswersOrder(questionId.Id) == false
                && _.GetMaxSelectedAnswerOptions(questionId.Id) == null
                && _.IsRosterSizeQuestion(questionId.Id) == true
            );

            var filteredOptionsViewModel = Setup.FilteredOptionsViewModel(new List<CategoricalOption>
            {
                Create.Entity.CategoricalQuestionOption(1, "item1"),
                Create.Entity.CategoricalQuestionOption(2, "item2"),
                Create.Entity.CategoricalQuestionOption(3, "item3"),
                Create.Entity.CategoricalQuestionOption(4, "item4"),
                Create.Entity.CategoricalQuestionOption(5, "item5"),
            });

            var yesNoAnswer = Create.Entity.InterviewTreeYesNoQuestion(new AnsweredYesNoOption[] {});
           
            var interview = Mock.Of<IStatefulInterview>(x => x.GetYesNoQuestion(questionId) == yesNoAnswer);

            var questionnaireStorage = new Mock<IQuestionnaireStorage>();
            interviewRepository = new Mock<IStatefulInterviewRepository>();

            questionnaireStorage.SetReturnsDefault(questionnaire);
            interviewRepository.Setup(x => x.Get(interviewIdAsString)).Returns(interview);
            answeringViewModelMock = new Mock<AnsweringViewModel>();
            answeringViewModelMock
                .Setup(x => x.SendAnswerQuestionCommandAsync(Moq.It.IsAny<AnswerQuestionCommand>()))
                .Callback((AnswerQuestionCommand command) => { answerCommand = command; })
                .Returns(Task.FromResult<bool>(true));

            viewModel = CreateViewModel(questionnaireStorage: questionnaireStorage.Object, 
                interviewRepository: interviewRepository.Object, 
                answeringViewModel: answeringViewModelMock.Object,
                filteredOptionsViewModel: filteredOptionsViewModel);

            viewModel.Init(interviewIdAsString, questionId, Create.Other.NavigationState());
            viewModel.Options.First().Selected = false;

            BecauseOf();
        }

        public void BecauseOf() => viewModel.ToggleAnswerAsync(viewModel.Options.First(), null).Wait();

        [NUnit.Framework.Test] public void should_send_answering_command () =>
            answeringViewModelMock.Verify(x => x.SendAnswerQuestionCommandAsync(Moq.It.IsAny<AnswerQuestionCommand>()), Times.Once);
        
        [NUnit.Framework.Test] public void should_send_command_with_toggled_first_option () =>
            ((AnswerYesNoQuestion)answerCommand).AnsweredOptions.Single().OptionValue.Should().Be(1);

        [NUnit.Framework.Test] public void should_send_command_with_toggled_NO_answer () =>
            ((AnswerYesNoQuestion)answerCommand).AnsweredOptions.Single().Yes.Should().BeFalse();

        private static AnswerQuestionCommand answerCommand;
        private static YesNoQuestionViewModel viewModel;
        private static Identity questionId;
        private static Guid questionGuid;
        private static Mock<IStatefulInterviewRepository> interviewRepository;
        private static string interviewIdAsString;
        private static Mock<AnsweringViewModel> answeringViewModelMock;
    }
}
