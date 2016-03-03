using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Entities.Interview;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.SingleOptionLinkedQuestionViewModelTests
{
    internal class when_handling_answer_removed_event_for_linked_question : SingleOptionLinkedQuestionViewModelTestsContext
    {
        Establish context = () =>
        {
            var questionnaire = SetupQuestionnaireWithSingleOptionQuestionLinkedToTextQuestion(questionId, linkedToQuestionId);

            var answer = new LinkedSingleOptionAnswer();

            var answeredLinkedOption = new[] { 1m };

            answer.SetAnswer(answeredLinkedOption);

            var interview = Mock.Of<IStatefulInterview>(_
                => _.FindAnswersOfReferencedQuestionForLinkedQuestion(Moq.It.IsAny<Guid>(), Moq.It.IsAny<Identity>()) == new[]
                {
                    Create.TextAnswer("answer not to remove", linkedToQuestionId, new [] { 0m }),
                    Create.TextAnswer("answer to remove", linkedToQuestionId, answeredLinkedOption),
                }
                   && _.Answers == new Dictionary<string, BaseInterviewAnswer>()
                   && _.GetLinkedSingleOptionAnswer(questionIdentity) == answer);

            viewModel = Create.SingleOptionLinkedQuestionViewModel(
                questionnaire: questionnaire,
                interview: interview);

            viewModel.Init(interviewId, questionIdentity, navigationState);
        };

        Because of = () =>
            viewModel.Handle(Create.Event.AnswerRemoved(questionIdentity));

        It should_set_IsAnswered_in_false = () =>
            viewModel.QuestionState.IsAnswered.ShouldBeFalse();

        It should_reset_all_options = () =>
            viewModel.Options.Select(x => x.Selected).ShouldEachConformTo(x => x == false);

        private static SingleOptionLinkedQuestionViewModel viewModel;
        private static string interviewId = "11111111111111111111111111111111";
        private static Guid questionId = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
        private static Identity questionIdentity = Create.Identity(questionId, Empty.RosterVector);
        private static Guid linkedToQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static NavigationState navigationState = Create.NavigationState();
    }
}