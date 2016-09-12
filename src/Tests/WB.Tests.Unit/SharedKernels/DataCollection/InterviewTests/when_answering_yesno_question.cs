using System;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_answering_yesno_question : with_event_context
    {
        Establish context = () =>
        {
            command = Create.Command.AnswerYesNoQuestion(
                userId: Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC"),
                questionId: Guid.Parse("11111111111111111111111111111111"),
                rosterVector: Create.Entity.RosterVector(1.1m),
                answeredOptions: new []
                {
                    Create.Entity.AnsweredYesNoOption(value: 1.1m, answer: true),
                    Create.Entity.AnsweredYesNoOption(value: 3.333m, answer: false),
                },
                answerTime: new DateTime(2015, 11, 19, 18, 04, 53));

            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new[]
            {
                Create.Entity.Roster(rosterId: rosterId, children: new[]
                {
                    Create.Entity.YesNoQuestion(questionId: command.QuestionId, answers: new[]
                    {
                        1.1m,
                        2.22m,
                        3.333m,
                    }),
                }),
            });

            interview = Setup.InterviewForQuestionnaireDocument(questionnaireDocument);
            interview.Apply(Create.Event.RosterInstancesAdded(rosterId: rosterId, fullRosterVectors: command.RosterVector));
        };

        Because of = () =>
            interview.AnswerYesNoQuestion(command);

        It should_raise_YesNoQuestionAnswered_event = () =>
            eventContext.ShouldContainEvent<YesNoQuestionAnswered>();

        It should_raise_YesNoQuestionAnswered_event_with_UserId_from_command = () =>
            eventContext.GetSingleEvent<YesNoQuestionAnswered>()
                .UserId.ShouldEqual(command.UserId);

        It should_raise_YesNoQuestionAnswered_event_with_QuestionId_from_command = () =>
            eventContext.GetSingleEvent<YesNoQuestionAnswered>()
                .QuestionId.ShouldEqual(command.QuestionId);

        It should_raise_YesNoQuestionAnswered_event_with_RosterVector_from_command = () =>
            eventContext.GetSingleEvent<YesNoQuestionAnswered>()
                .RosterVector.ShouldEqual(command.RosterVector);

        It should_raise_YesNoQuestionAnswered_event_with_AnsweredOptions_from_command = () =>
            eventContext.GetSingleEvent<YesNoQuestionAnswered>()
                .AnsweredOptions.ShouldEqual(command.AnsweredOptions);

        It should_raise_YesNoQuestionAnswered_event_with_AnswerTime_from_command = () =>
            eventContext.GetSingleEvent<YesNoQuestionAnswered>()
                .AnswerTimeUtc.ShouldEqual(command.AnswerTime);

        private static AnswerYesNoQuestion command;
        private static Interview interview;
        private static Guid rosterId = Guid.Parse("44444444444444444444444444444444");
    }
}