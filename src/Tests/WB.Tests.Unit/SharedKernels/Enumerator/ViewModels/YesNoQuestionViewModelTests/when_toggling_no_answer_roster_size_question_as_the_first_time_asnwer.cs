using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.YesNoQuestionViewModelTests
{
    internal class when_toggling_no_answer_roster_size_question_as_the_first_time_asnwer : YesNoQuestionViewModelTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionId = Create.Entity.Identity("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", Empty.RosterVector);
           
            var filteredOptionsViewModel = Abc.SetUp.FilteredOptionsViewModel(new List<CategoricalOption>
            {
                Create.Entity.CategoricalQuestionOption(1, "item1"),
                Create.Entity.CategoricalQuestionOption(2, "item2"),
                Create.Entity.CategoricalQuestionOption(3, "item3"),
                Create.Entity.CategoricalQuestionOption(4, "item4"),
                Create.Entity.CategoricalQuestionOption(5, "item5"),
            });

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.YesNoQuestion(questionId.Id, new []{1,2,3,4,5}),
                Create.Entity.MultiRoster(Id.g2, rosterSizeQuestionId: questionId.Id)
            );
            var questionnaireStorage = SetUp.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire);

            interview = SetUp.StatefulInterview(questionnaire);
            var interviewRepository = SetUp.StatefulInterviewRepository(interview);

            answeringViewModelMock = new Mock<AnsweringViewModel>();
            answeringViewModelMock
                .Setup(x => x.SendQuestionCommandAsync(Moq.It.IsAny<AnswerQuestionCommand>()))
                .Callback((AnswerQuestionCommand command) => { answerCommand = command; })
                .Returns(Task.FromResult<bool>(true));

            eventRegistry = Create.Service.LiteEventRegistry();

            viewModel = CreateViewModel(questionnaireStorage: questionnaireStorage, 
                interviewRepository: interviewRepository, 
                answeringViewModel: answeringViewModelMock.Object,
                filteredOptionsViewModel: filteredOptionsViewModel,
                eventRegistry: eventRegistry);

            viewModel.Init(interview.Id.FormatGuid(), questionId, Create.Other.NavigationState());
            

            BecauseOf();
        }

        public void BecauseOf()
        {
            var yesNoOption = viewModel.Options.First() as CategoricalYesNoOptionViewModel;
            yesNoOption.NoSelected = true;
            yesNoOption.SetNoAnswerCommand.Execute();
            interview.AnswerYesNoQuestion(Create.Command.AnswerYesNoQuestion(null, questionId.Id, questionId.RosterVector, new[]
            {
                Create.Entity.AnsweredYesNoOption(1, false)
            }));
            SetUp.ApplyInterviewEventsToViewModels(interview, eventRegistry, interview.Id);
        }

        [NUnit.Framework.Test] public void should_send_answering_command () =>
            answeringViewModelMock.Verify(x => x.SendQuestionCommandAsync(Moq.It.IsAny<AnswerQuestionCommand>()), Times.Once);
        
        [NUnit.Framework.Test] public void should_send_command_with_toggled_first_option () =>
            ((AnswerYesNoQuestion)answerCommand).AnsweredOptions.Single().OptionValue.Should().Be(1);

        [NUnit.Framework.Test] public void should_send_command_with_toggled_NO_answer () =>
            ((AnswerYesNoQuestion)answerCommand).AnsweredOptions.Single().Yes.Should().BeFalse();

        private static AnswerQuestionCommand answerCommand;
        private static CategoricalYesNoViewModel viewModel;
        private static Identity questionId;
        private static Mock<AnsweringViewModel> answeringViewModelMock;
        private static StatefulInterview interview;
        private static IViewModelEventRegistry eventRegistry;
    }
}
