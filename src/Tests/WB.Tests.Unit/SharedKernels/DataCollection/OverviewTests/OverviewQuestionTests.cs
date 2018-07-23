using System;
using System.Collections.Generic;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Views.Interview.Overview;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.OverviewTests
{
    [TestOf(typeof(OverviewQuestion))]
    public class OverviewQuestionTests
    {
        private StatefulInterview interview;
        private readonly Guid questionId = Id.g1;

        [SetUp]
        public void Setup()
        {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.TextQuestion(questionId, validationConditions: new List<ValidationCondition>()
                {
                    Create.Entity.ValidationCondition()
                })
            );

            interview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaire);
        }


        [Test]
        public void should_set_unanswered_when_question_is_unanswered()
        {
            var identity = Create.Identity(questionId);
            var node = new OverviewQuestion(interview.GetQuestion(identity), interview);

            Assert.That(node.Answer, Is.Null);
            Assert.That(node.IsAnswered, Is.False);
            Assert.That(node.ErrorMessages, Is.Empty);
            Assert.That(node.HasWarnings, Is.False);
            Assert.That(node.State, Is.EqualTo(OverviewNodeState.Unanswered));
            Assert.That(node.Id, Is.EqualTo(identity.ToString()));
        }

        [Test]
        public void should_set_invalid_when_question_is_invalid()
        {
            var identity = Create.Identity(questionId);
            interview.AnswerTextQuestion(Id.gA, identity.Id, identity.RosterVector, DateTime.Now, "answer");

            interview.ApplyEvent(Create.Event.AnswersDeclaredInvalid(identity));

            var node = new OverviewQuestion(interview.GetQuestion(identity), interview);

            Assert.That(node.Answer, Is.EqualTo("answer"));
            Assert.That(node.HasWarnings, Is.False);
            Assert.That(node.State, Is.EqualTo(OverviewNodeState.Invalid));
        }

        [Test]
        public void should_set_state_to_answered()
        {
            var identity = Create.Identity(questionId);
            interview.AnswerTextQuestion(Id.gA, identity.Id, identity.RosterVector, DateTime.Now, "answer");

            var node = new OverviewQuestion(interview.GetQuestion(identity), interview);

            Assert.That(node.Answer, Is.EqualTo("answer"));
            Assert.That(node.HasWarnings, Is.False);
            Assert.That(node.State, Is.EqualTo(OverviewNodeState.Answered));
        }

        [Test]
        public void should_set_state_to_commented()
        {
            var identity = Create.Identity(questionId);
            interview.CommentAnswer(Id.gA,  identity.Id, identity.RosterVector, DateTime.Now, "comment");

            var node = new OverviewQuestion(interview.GetQuestion(identity), interview);

            Assert.That(node.HasComment, Is.True);
        }
    }
}
