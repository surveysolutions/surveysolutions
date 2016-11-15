using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Moq;
using Nito.AsyncEx.Synchronous;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.Enumerator.Aggregates;

using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.SingleOptionLinkedQuestionViewModelTests
{
    internal class when_initializing : SingleOptionLinkedQuestionViewModelTestsContext
    {
        Establish context = () =>
        {
            var questionnaire = SetupQuestionnaireWithSingleOptionQuestionLinkedToTextQuestion(questionId, linkedToQuestionId);

            var interview = Mock.Of<IStatefulInterview>(_
                => _.GetLinkedSingleOptionQuestion(Moq.It.IsAny<Identity>()) == new InterviewTreeSingleLinkedOptionQuestion(new[]
                    {
                        Create.Entity.InterviewTreeTextQuestion("answer1"),
                        Create.Entity.InterviewTreeTextQuestion(null),
                        Create.Entity.InterviewTreeTextQuestion("answer2"),
                    })
                && _.Answers == new Dictionary<string, BaseInterviewAnswer>());

            viewModel = Create.ViewModel.SingleOptionLinkedQuestionViewModel(
                eventRegistry: eventRegistryMock.Object,
                questionState: questionStateMock.Object,
                questionnaire: questionnaire,
                interview: interview);
        };

        Because of = () =>
            viewModel.Init(interviewId, questionIdentity, navigationState);

        It should_initialize_question_state = () =>
            questionStateMock.Verify(state => state.Init(interviewId, questionIdentity, navigationState), Times.Once);

        It should_subsribe_self_to_event_registry = () =>
            eventRegistryMock.Verify(registry => registry.Subscribe(viewModel, Moq.It.IsAny<string>()), Times.Once);

        It should_fill_options_with_answers_from_linked_to_question = () =>
            viewModel.Options.Select(option => option.Title).ShouldContainOnly("answer1", "answer2");

        private static Mock<ILiteEventRegistry> eventRegistryMock = new Mock<ILiteEventRegistry>();
        private static SingleOptionLinkedQuestionViewModel viewModel;
        private static string interviewId = "11111111111111111111111111111111";
        private static Guid questionId = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
        private static Identity questionIdentity = Create.Entity.Identity(questionId, Empty.RosterVector);
        private static Guid linkedToQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static NavigationState navigationState = Create.Other.NavigationState();
        private static Mock<QuestionStateViewModel<SingleOptionLinkedQuestionAnswered>> questionStateMock = new Mock<QuestionStateViewModel<SingleOptionLinkedQuestionAnswered>>();
    }
}