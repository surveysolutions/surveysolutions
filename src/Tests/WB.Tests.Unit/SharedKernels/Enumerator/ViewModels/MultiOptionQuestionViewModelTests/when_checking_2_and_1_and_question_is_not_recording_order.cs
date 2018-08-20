using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.MultiOptionQuestionViewModelTests
{
    internal class when_checking_2_and_1_and_question_is_not_recording_order : MultiOptionQuestionViewModelTestsContext
    {
        [OneTimeSetUp] 
        public void context () {
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
            });

            var multiOptionAnswer = Create.Entity.InterviewTreeMultiOptionQuestion(new[] { 2m });

            var interview = Mock.Of<IStatefulInterview>(x => x.GetMultiOptionQuestion(questionId) == multiOptionAnswer);

            var questionnaireStorage = Stub<IQuestionnaireStorage>.Returning(questionnaire);
            var interviewRepository = Stub<IStatefulInterviewRepository>.Returning(interview);

            answeringMock = new Mock<AnsweringViewModel>();
            answeringMock
                .Setup<Task>(answeringViewModel => answeringViewModel.SendAnswerQuestionCommandAsync(Moq.It.IsAny<AnswerQuestionCommand>()))
                .Returns<AnswerQuestionCommand>(x => Task.CompletedTask)
                .Callback(delegate(AnswerQuestionCommand command)
                {
                    executedCommand = command;
                });

            viewModel = CreateViewModel(questionnaireStorage: questionnaireStorage,
                interviewRepository: interviewRepository,
                answeringViewModel: answeringMock.Object,
                filteredOptionsViewModel: filteredOptionsViewModel);

            viewModel.Init("blah", questionId, Create.Other.NavigationState());
            BecauseOf();
        }

        public void BecauseOf() 
        {
            viewModel.Options.First().Checked = true;
            viewModel.ToggleAnswerAsync(viewModel.Options.First()).WaitAndUnwrapException();
        }

        [Test] 
        public void should_execute_command_with_checked_values_1_and_2 () 
            => ((AnswerMultipleOptionsQuestionCommand) executedCommand).SelectedValues.Should().BeEquivalentTo(new[] { 1, 2 });

        static MultiOptionQuestionViewModel viewModel;
        static Identity questionId;
        static Guid questionGuid;
        static Mock<AnsweringViewModel> answeringMock;
        private static AnswerQuestionCommand executedCommand;
    }
}
