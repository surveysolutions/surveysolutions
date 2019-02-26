using System;
using System.Collections.Generic;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.Services
{
    [TestOf(typeof(EnumeratorGroupGroupStateCalculationStrategy))]
    public class EnumeratorGroupGroupStateCalculationStrategyTests
    {
        [Test]
        public void when_group_has_unanswered_hidden_question()
        {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.TextListQuestion(Id.gA, variable: "list"),
                Create.Entity.ListRoster(Id.gB, rosterSizeQuestionId: Id.gA, children: new[]
                {
                    Create.Entity.TextQuestion(Id.g1),
                    Create.Entity.TextQuestion(Id.g2, scope: QuestionScope.Hidden)
                }));

            var interview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaire, 
                answers: new List<InterviewAnswer>
                {
                    Create.Entity.InterviewAnswer(Create.Identity(Id.gA), Create.Entity.ListAnswer(1)),
                    Create.Entity.InterviewAnswer(Create.Identity(Id.g2, Create.RosterVector(1)), Create.Entity.TextQuestionAnswer("hidden b"))
                },
                protectedAnswers: new List<string> { "list" });

            var strategy = Create.Service.EnumeratorGroupGroupStateCalculationStrategy();

            // Act
            interview.AnswerTextListQuestion(Id.gC, Id.gA, RosterVector.Empty, DateTimeOffset.UtcNow, new []{ new Tuple<decimal, string>(1, "answer #1"), new Tuple<decimal, string>(2, "bbb") });
            interview.AnswerTextQuestion(Id.gC, Id.g1, Create.RosterVector(2), DateTimeOffset.UtcNow, "b");

            GroupStatus calculateDetailedStatus = strategy.CalculateDetailedStatus(Create.Identity(Id.gB, Create.RosterVector(2)), interview, Create.Entity.PlainQuestionnaire(questionnaire));

            // Assert
            Assert.That(calculateDetailedStatus, Is.EqualTo(GroupStatus.Completed));
        }

        [Test]
        public void should_mark_group_as_started_when_there_is_one_answered_and_one_unanswered()
        {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(chapterId: Id.gA,
                children: new IComposite[]
                {
                    Create.Entity.TextQuestion(Id.g1),
                    Create.Entity.TextQuestion(Id.g2)
                });

            var interview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaire);
            var strategy = Create.Service.EnumeratorGroupGroupStateCalculationStrategy();

            // Act
            interview.AnswerTextQuestion(Id.gC, Id.g1, RosterVector.Empty, DateTimeOffset.UtcNow, "a");

            GroupStatus calculateDetailedStatus = strategy.CalculateDetailedStatus(Create.Identity(Id.gA), interview, Create.Entity.PlainQuestionnaire(questionnaire));

            // Assert
            Assert.That(calculateDetailedStatus, Is.EqualTo(GroupStatus.Started));
        }
    }
}
