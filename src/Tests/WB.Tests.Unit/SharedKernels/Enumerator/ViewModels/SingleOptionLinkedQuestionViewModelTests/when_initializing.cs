using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Moq;
using Nito.AsyncEx.Synchronous;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.SingleOptionLinkedQuestionViewModelTests
{
    [Ignore("KP-8159")]
    internal class when_initializing : SingleOptionLinkedQuestionViewModelTestsContext
    {
        Establish context = () =>
        {
            linkSourceQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            linkedQuestionId = Create.Entity.Identity(Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB"), RosterVector.Empty);
            interviewId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC").FormatGuid();
            interviewerId = Guid.Parse("77777777777777777777777777777777");

            var questionnaire = SetupQuestionnaireWithSingleOptionQuestionLinkedToTextQuestion(linkedQuestionId.Id, linkSourceQuestionId);

            interview = Setup.StatefulInterview(questionnaire);

            interview.AnswerTextQuestion(interviewerId, linkSourceQuestionId, Create.Entity.RosterVector(1), DateTime.UtcNow, "answer1");
            interview.AnswerTextQuestion(interviewerId, linkSourceQuestionId, Create.Entity.RosterVector(2), DateTime.UtcNow, "answer2");


            viewModel = Create.ViewModel.SingleOptionLinkedQuestionViewModel(
                eventRegistry: eventRegistryMock.Object,
                questionState: questionStateMock.Object,
                questionnaire: Create.Entity.PlainQuestionnaire(questionnaire),
                interview: interview);
        };

        Because of = () =>
            viewModel.Init(interviewId, linkedQuestionId, navigationState);

        It should_initialize_question_state = () =>
            questionStateMock.Verify(state => state.Init(interviewId, linkedQuestionId, navigationState), Times.Once);

        It should_subsribe_self_to_event_registry = () =>
            eventRegistryMock.Verify(registry => registry.Subscribe(viewModel, Moq.It.IsAny<string>()), Times.Once);

        It should_fill_options_with_answers_from_linked_to_question = () =>
            viewModel.Options.Select(option => option.Title).ShouldContainOnly("answer1", "answer2");

        private static Mock<ILiteEventRegistry> eventRegistryMock = new Mock<ILiteEventRegistry>();
        private static SingleOptionLinkedQuestionViewModel viewModel;
        static Guid linkSourceQuestionId;
        static Identity linkedQuestionId;
        static string interviewId;
        static StatefulInterview interview;
        static Guid interviewerId;
        private static NavigationState navigationState = Create.Other.NavigationState();
        private static Mock<QuestionStateViewModel<SingleOptionLinkedQuestionAnswered>> questionStateMock = new Mock<QuestionStateViewModel<SingleOptionLinkedQuestionAnswered>>();
    }
}