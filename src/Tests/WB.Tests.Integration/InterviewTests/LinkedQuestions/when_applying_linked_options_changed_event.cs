using System;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;

namespace WB.Tests.Integration.InterviewTests.LinkedQuestions
{
    [TestOf(typeof(Interview))]
    internal class when_applying_linked_options_changed_event_and_linked_question_from_removed_roster : InterviewTestsContext
    {
        [Test]
        public void should_not_throw_exception()
        {
            var userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFF1111111111");
            var textListQuestionId = Guid.Parse("11111111111111111111111111111111");
            var textListRosterId =   Guid.Parse("22222222222222222222222222222222");
            var singleLinkedToListRosterId = Guid.Parse("33333333333333333333333333333333");

            var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
               Abc.Create.Entity.TextListQuestion(textListQuestionId),
               Abc.Create.Entity.Roster(textListRosterId, rosterSizeSourceType: RosterSizeSourceType.Question, rosterSizeQuestionId: textListQuestionId, children:new []
               {
                   Abc.Create.Entity.SingleOptionQuestion(singleLinkedToListRosterId, linkedToRosterId: textListRosterId)
               })
            });

            var interview = SetupStatefullInterview(questionnaireDocument);

            interview.AnswerTextListQuestion(userId: userId, questionId: textListQuestionId,
                originDate: DateTimeOffset.Now, rosterVector: new decimal[0],
                answers: new[] {new Tuple<decimal, string>(1, "a"), new Tuple<decimal, string>(2, "b"), new Tuple<decimal, string>(3, "c") });

            interview.AnswerTextListQuestion(userId: userId, questionId: textListQuestionId,
                originDate: DateTimeOffset.Now, rosterVector: new decimal[0],
                answers: new[] { new Tuple<decimal, string>(1, "a"), new Tuple<decimal, string>(3, "c") });

            Assert.DoesNotThrow(() => interview.Apply(Abc.Create.Event.LinkedOptionsChanged(new [] {
                new ChangedLinkedOptions(Identity.Create(singleLinkedToListRosterId, Abc.Create.Entity.RosterVector(new[] {1})), new[] { Abc.Create.Entity.RosterVector(new[] {1}), Abc.Create.Entity.RosterVector(new[] {3}) }),
                new ChangedLinkedOptions(Identity.Create(singleLinkedToListRosterId, Abc.Create.Entity.RosterVector(new[] {2})), new[] { Abc.Create.Entity.RosterVector(new[] {1}), Abc.Create.Entity.RosterVector(new[] {3}) }),
                new ChangedLinkedOptions(Identity.Create(singleLinkedToListRosterId, Abc.Create.Entity.RosterVector(new[] {3})), new[] { Abc.Create.Entity.RosterVector(new[] {1}), Abc.Create.Entity.RosterVector(new[] {3}) }),
            })));
        }
    }
}
