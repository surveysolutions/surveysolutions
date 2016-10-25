using System;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_answering_yesno_question : with_event_context
    {
        Establish context = () =>
        {
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.NumericIntegerQuestion(numericId),
                Create.Entity.Roster(rosterId: rosterId, rosterSizeQuestionId: numericId, children: new[]
                {
                    Create.Entity.YesNoQuestion(questionId: questionId, answers: new[] { 11m, 222m, 3333m, })
                }),
            });

            interview = Setup.InterviewForQuestionnaireDocument(questionnaireDocument);
            interview.Apply(Create.Event.NumericIntegerQuestionAnswered(numericId, Empty.RosterVector, 1));
            interview.Apply(Create.Event.RosterInstancesAdded(rosterId, Create.Entity.RosterVector(0)));
        };

        Because of = () =>
            interview.AnswerYesNoQuestion(Create.Command.AnswerYesNoQuestion(
                userId: userId,
                questionId: questionId,
                rosterVector: Create.Entity.RosterVector(0),
                answeredOptions: answeredYesNoOptions,
                answerTime: DateTime.UtcNow));

        It should_raise_YesNoQuestionAnswered_event = () =>
            eventContext.ShouldContainEvent<YesNoQuestionAnswered>();

        It should_raise_YesNoQuestionAnswered_event_with_UserId_from_command = () =>
            eventContext.GetEvent<YesNoQuestionAnswered>().UserId.ShouldEqual(userId);

        It should_raise_YesNoQuestionAnswered_event_with_QuestionId_from_command = () =>
            eventContext.GetSingleEvent<YesNoQuestionAnswered>()
                .QuestionId.ShouldEqual(questionId);

        It should_raise_YesNoQuestionAnswered_event_with_RosterVector_from_command = () =>
            eventContext.GetSingleEvent<YesNoQuestionAnswered>()
                .RosterVector.ShouldContainOnly(0);

        It should_raise_YesNoQuestionAnswered_event_with_AnsweredOptions_from_command = () =>
            eventContext.GetSingleEvent<YesNoQuestionAnswered>()
                .AnsweredOptions.ShouldEqual(answeredYesNoOptions);

        It should_raise_YesNoQuestionAnswered_event_with_AnswerTime_from_command = () =>
            (DateTime.UtcNow - eventContext.GetSingleEvent<YesNoQuestionAnswered>().AnswerTimeUtc).Seconds.ShouldBeLessThan(2);

        private static Interview interview;
        private static readonly Guid rosterId = Guid.Parse("44444444444444444444444444444444");
        private static readonly Guid numericId =Guid.Parse("55555555555555555555555555555555");
        private static readonly Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid userId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");

        private static readonly AnsweredYesNoOption[] answeredYesNoOptions = new[]
        {
            Create.Entity.AnsweredYesNoOption(value: 11m, answer: true),
            Create.Entity.AnsweredYesNoOption(value: 3333m, answer: false),
        };
    }
}