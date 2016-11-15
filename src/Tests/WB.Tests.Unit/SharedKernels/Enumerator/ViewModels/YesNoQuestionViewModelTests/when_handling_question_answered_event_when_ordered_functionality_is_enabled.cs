using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Moq;
using Nito.AsyncEx.Synchronous;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.YesNoQuestionViewModelTests
{
    internal class when_handling_question_answered_event_when_ordered_functionality_is_enabled : YesNoQuestionViewModelTestsContext
    {
        Establish context = () =>
        {
            questionGuid = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            questionId = Create.Entity.Identity(questionGuid, Empty.RosterVector);

            var questionnaire = Mock.Of<IQuestionnaire>(_
                => _.ShouldQuestionRecordAnswersOrder(questionId.Id) == true
                && _.GetMaxSelectedAnswerOptions(questionId.Id) == null
                && _.ShouldQuestionSpecifyRosterSize(questionId.Id) == false
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
                new AnsweredYesNoOption(1, false),
                new AnsweredYesNoOption(4, false),
                new AnsweredYesNoOption(3, true),
                new AnsweredYesNoOption(2, true),
            });

            var interview = Mock.Of<IStatefulInterview>(x => x.GetYesNoQuestion(questionId) == yesNoAnswer);
            
            var questionnaireStorage = new Mock<IQuestionnaireStorage>();
            var interviewRepository = new Mock<IStatefulInterviewRepository>();
            answering = new Mock<AnsweringViewModel>();

            questionnaireStorage.SetReturnsDefault(questionnaire);
            interviewRepository.SetReturnsDefault(interview);

            viewModel = CreateViewModel(questionnaireStorage: questionnaireStorage.Object,
                interviewRepository: interviewRepository.Object,
                answeringViewModel: answering.Object,
                filteredOptionsViewModel: filteredOptionsViewModel);

            viewModel.Init("blah", questionId, Create.Other.NavigationState());
        };

        Because of = () =>
        {
            viewModel.Options.Single(o => o.Value == 4).YesSelected = true;
        };


        It should_send_answers_to_command_service = () =>
        {
            answering.Verify(s => s.SendAnswerQuestionCommandAsync(Moq.It.IsAny<AnswerYesNoQuestion>()), Times.Once());
        };

        It should_send_answers_in_correct_order = () =>
        {
            answering.Verify(s => s.SendAnswerQuestionCommandAsync(Moq.It.Is<AnswerYesNoQuestion>(
                c =>
                    c.AnsweredOptions.Length == 5
                    && c.AnsweredOptions[0].Yes == true  && c.AnsweredOptions[0].OptionValue == 5
                    && c.AnsweredOptions[1].Yes == false && c.AnsweredOptions[1].OptionValue == 1
                    && c.AnsweredOptions[2].Yes == true  && c.AnsweredOptions[2].OptionValue == 3
                    && c.AnsweredOptions[3].Yes == true  && c.AnsweredOptions[3].OptionValue == 2
                    && c.AnsweredOptions[4].Yes == true  && c.AnsweredOptions[4].OptionValue == 4
                )), 
                Times.Once());
        };

        static Mock<AnsweringViewModel> answering;
        static YesNoQuestionViewModel viewModel;
        static Identity questionId;
        private static Guid questionGuid;
    }
}