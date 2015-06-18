using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Aggregates;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities.QuestionModels;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionStateViewModels;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.QuestionnaireTester.ViewModels.SingleOptionLinkedQuestionViewModelTests
{
    public class when_handling_answer_removed_event : SingleOptionLinkedQuestionViewModelTestsContext
    {
        Establish context = () =>
        {
            var questionnaire = SetupQuestionnaireModelWithSingleOptionQuestionLinkedToTextQuestion(questionId, linkedToQuestionId);

            var interview = Mock.Of<IStatefulInterview>(_
                => _.FindAnswersOfLinkedToQuestionForLinkedQuestion(Moq.It.IsAny<Guid>(), Moq.It.IsAny<Identity>()) == new[]
                {
                    Create.TextAnswer("answer not to remove", linkedToQuestionId, new [] { 0m }),
                    Create.TextAnswer("answer to remove", linkedToQuestionId, answerToRemoveRosterVector),
                }
                && _.Answers == new Dictionary<string, BaseInterviewAnswer>());

            viewModel = Create.SingleOptionLinkedQuestionViewModel(
                questionnaireModel: questionnaire,
                interview: interview);

            viewModel.Init(interviewId, questionIdentity, navigationState);
        };

        Because of = () =>
            viewModel.Handle(Create.Event.AnswersRemoved(Create.Event.Identity(linkedToQuestionId, answerToRemoveRosterVector)));

        It should_remove_removed_answer_from_options = () =>
            viewModel.Options.Select(option => option.Title).ShouldContainOnly("answer not to remove");

        private static SingleOptionLinkedQuestionViewModel viewModel;
        private static string interviewId = "11111111111111111111111111111111";
        private static Guid questionId = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
        private static Identity questionIdentity = Create.Identity(questionId, Empty.RosterVector);
        private static Guid linkedToQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static NavigationState navigationState = Create.NavigationState();
        private static decimal[] answerToRemoveRosterVector = { 1m };
    }
}