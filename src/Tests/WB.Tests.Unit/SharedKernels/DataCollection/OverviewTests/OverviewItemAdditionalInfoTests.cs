using System;
using System.Collections.Generic;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Views.Interview.Overview;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.OverviewTests
{
    [TestOf(typeof(OverviewItemAdditionalInfo))]
    public class OverviewItemAdditionalInfoTests
    {
        private StatefulInterview interview;
        private readonly Guid questionId = Id.g1;
        private readonly Identity identity = Create.Identity(Id.g1);
        [SetUp]
        public void Setup()
        {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.TextQuestion(questionId, validationConditions: new List<ValidationCondition>()
                {
                    Create.Entity.ValidationCondition(),
                    Create.Entity.WarningCondition()
                })
            );

            //16:30:59 UTC
            var time_12_30_in_DC = new DateTimeOffset(2018, 10, 28, 12, 30, 59, new TimeSpan(-4, 0, 0));
            //16:31:59 UTC
            var time_22_01_in_IN = new DateTimeOffset(2018, 10, 28, 22, 01, 59, new TimeSpan(5, 30, 0));

            interview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaire);
            interview.Apply(Create.Event.AnswersDeclaredImplausible(identity, new []{ 1 }));
            interview.Apply(Create.Event.AnswerDeclaredInvalid(identity, new []{ 0 }));
            interview.CommentAnswer(Id.gA, questionId, RosterVector.Empty, time_12_30_in_DC, "Commented from Washington DC at 12:30");
            interview.CommentAnswer(Id.gB, questionId, RosterVector.Empty, time_22_01_in_IN, "Commented from India at 22:01");
        }

        [Test]
        public void should_set_UTC_time_for_comments_from_different_timezone()
        {

            var additionalInfo = new OverviewItemAdditionalInfo(interview.GetQuestion(identity), interview, Id.gA);

            Assert.That(additionalInfo.Comments.Count, Is.EqualTo(2));

            // Discussed with Zurab, this is the result of discussion:
            Assert.That(
                additionalInfo.Comments[0].CommentTimeUtc, 
                Is.EqualTo(new DateTime(2018, 10, 28, 16, 30, 59)),
                "12:30 DC time should be converted to 16:30UTC time and both user from DC should see 12:30 and from India 22:00");

            Assert.That(
                additionalInfo.Comments[1].CommentTimeUtc, 
                Is.EqualTo(new DateTime(2018, 10, 28, 16, 31, 59)),
                "22:01 IN time should be converted to 16:31UTC time and both user from DC should see 12:31 and from India 22:01");

            Assert.That(
                additionalInfo.Comments[1].CommentTimeUtc - additionalInfo.Comments[0].CommentTimeUtc, 
                Is.EqualTo(new TimeSpan(0, 0, 0, 60)), 
                "Should be 1 minute apart");
        }

        [Test]
        public void should_set_comments()
        {
            var additionalInfo = new OverviewItemAdditionalInfo(interview.GetQuestion(identity), interview, Id.gA);

            Assert.That(additionalInfo.Comments.Count, Is.EqualTo(2));
            Assert.That(additionalInfo.Comments[0].IsOwnComment, Is.True);
            Assert.That(additionalInfo.Comments[0].Text, Is.EqualTo("Commented from Washington DC at 12:30"));

            Assert.That(additionalInfo.Comments[1].IsOwnComment, Is.False);
            Assert.That(additionalInfo.Comments[1].Text, Is.EqualTo("Commented from India at 22:01"));
        }

        [Test]
        public void should_set_validation_errors()
        {
            var additionalInfo = new OverviewItemAdditionalInfo(interview.GetQuestion(identity), interview, Id.gA);

            CollectionAssert.AreEqual(new[] { "should be answered [1]" }, additionalInfo.Errors);
        }

        [Test]
        public void should_set_validation_warnings()
        {
            var additionalInfo = new OverviewItemAdditionalInfo(interview.GetQuestion(identity), interview, Id.gA);

            CollectionAssert.AreEqual(new[] { "warning about unanswered question [2]" }, additionalInfo.Warnings);
        }
    }
}
