using System;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.BoundedContexts.Supervisor.ViewModel;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Supervisor.ViewModels
{
    [TestOf(typeof(SupervisorGroupStateCalculationStrategy))]
    public class SupervisorGroupStateCalculationStrategyTests
    {
        private Guid userId = Id.g2;

        [Test]
        public void when_group_supervisor_question_should_set_state_to_not_started()
        {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                chapterId: Id.g1,
                children: Create.Entity.TextQuestion(Id.gA, scope: QuestionScope.Supervisor)
            );

            var interview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaire);

            var strategy = Create.Service.SupervisorGroupStateCalculationStrategy();
            
            // Act
            GroupStatus status = strategy.CalculateDetailedStatus(Create.Identity(Id.g1), interview);

            // Assert
            Assert.That(status, Is.EqualTo(GroupStatus.NotStarted));
        }

        [Test]
        public void when_group_has_one_answered_and_one_unanswered_question_Should_set_state_to_Started()
        {
            var answeredQuestionId = Id.gB;
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                chapterId: Id.g1,
                children: 
                    new IComposite[]
                    {
                        Create.Entity.TextQuestion(Id.gA, scope: QuestionScope.Supervisor),
                        Create.Entity.TextQuestion(answeredQuestionId)
                    }
            );

            var interview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaire);
            interview.AnswerTextQuestion(userId, answeredQuestionId, RosterVector.Empty, DateTime.UtcNow, "answer");

            var strategy = Create.Service.SupervisorGroupStateCalculationStrategy();
            
            // Act
            GroupStatus status = strategy.CalculateDetailedStatus(Create.Identity(Id.g1), interview);

            // Assert
            Assert.That(status, Is.EqualTo(GroupStatus.Started));
        }

        [Test]
        public void when_group_has_all_questions_answered_Should_set_state_to_completed()
        {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                chapterId: Id.g1,
                children: Create.Entity.TextQuestion(Id.gA, scope: QuestionScope.Supervisor)
            );

            var interview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaire);
            interview.AnswerTextQuestion(userId, Id.gA, RosterVector.Empty, DateTime.Now, "answer");

            var strategy = Create.Service.SupervisorGroupStateCalculationStrategy();
            
            // Act
            GroupStatus status = strategy.CalculateDetailedStatus(Create.Identity(Id.g1), interview);

            // Assert
            Assert.That(status, Is.EqualTo(GroupStatus.Completed));
        }

        [Test]
        public void when_subgroup_has_not_anwered_questions_should_set_state_to_Started()
        {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                chapterId: Id.g1,
                children: 
                    Create.Entity.Group(
                        children: new IComposite[]{Create.Entity.TextQuestion(Id.gA, scope: QuestionScope.Supervisor)})
            );

            var interview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaire);

            var strategy = Create.Service.SupervisorGroupStateCalculationStrategy();
            
            // Act
            GroupStatus status = strategy.CalculateDetailedStatus(Create.Identity(Id.g1), interview);

            // Assert
            Assert.That(status, Is.EqualTo(GroupStatus.Started));
        }

        [Test]
        public void when_group_is_completed_and_subgroup_is_completed_should_set_state_to_Completed()
        {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                chapterId: Id.g1,
                children: 
                Create.Entity.Group(
                    children: new IComposite[]{Create.Entity.TextQuestion(Id.gA, scope: QuestionScope.Supervisor)})
            );

            var interview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaire);
            interview.AnswerTextQuestion(userId, Id.gA, RosterVector.Empty, DateTime.Now, "answer");

            var strategy = Create.Service.SupervisorGroupStateCalculationStrategy();
            
            // Act
            GroupStatus status = strategy.CalculateDetailedStatus(Create.Identity(Id.g1), interview);

            // Assert
            Assert.That(status, Is.EqualTo(GroupStatus.Completed));
        }

        [Test]
        public void when_group_is_not_started_but_subgroup_is_started_Should_return_Started_status()
        {
            Guid answeredQuestionId = Id.gB;

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                chapterId: Id.g1,
                children: 
                Create.Entity.Group(
                    children: new IComposite[]
                    {
                        Create.Entity.TextQuestion(Id.gA, scope: QuestionScope.Supervisor),
                        Create.Entity.TextQuestion(answeredQuestionId)
                    })
            );

            var interview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaire);
            interview.AnswerTextQuestion(userId, answeredQuestionId, RosterVector.Empty, DateTime.UtcNow, "answer");

            var strategy = Create.Service.SupervisorGroupStateCalculationStrategy();
            
            // Act
            GroupStatus status = strategy.CalculateDetailedStatus(Create.Identity(Id.g1), interview);

            // Assert
            Assert.That(status, Is.EqualTo(GroupStatus.Started));
        }
    }
}
