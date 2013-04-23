using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Complete;
using Main.Core.Entities.SubEntities.Question;
using NUnit.Framework;

namespace Main.Core.Tests.Documents
{
    [TestFixture]
    public class QuestionnaireDocumentTests
    {
        [Test]
        public void RemoveGroup_when_AutoPropagate_group_that_was_trigged_by_AuotoPropagate_question_was_removed_then_list_of_triggers_should_be_one_less()
        {
            // Arrange
            var autoGroupId = Guid.NewGuid();
            AutoPropagateQuestion autoQuestion;
            var doc = this.CreateQuestionnaireDocumentWithAutoPropagateGroupAndQuestion(autoGroupId, out autoQuestion);
            var autoQuestionTriggersCount = autoQuestion.Triggers.Count;

            // Act
            doc.RemoveGroup(autoGroupId);

            // Assert
            Assert.That(autoQuestion.Triggers.Count, Is.EqualTo(autoQuestionTriggersCount - 1));
        }

        [Test]
        public void RemoveGroup_when_AutoPropagate_group_removed_then_triggers_in_AutoPropagate_question_should_not_contains_id_of_deleted_group()
        {
            // Arrange
            var autoPropagateGroupId = Guid.NewGuid();
            AutoPropagateQuestion autoQuestion;
            var doc = this.CreateQuestionnaireDocumentWithAutoPropagateGroupAndQuestion(autoPropagateGroupId, out autoQuestion);

            // Act
            doc.RemoveGroup(autoPropagateGroupId);

            // Assert
            Assert.That(autoQuestion.Triggers, !Contains.Item(autoPropagateGroupId));
        }

        [Test]
        public void RemoveGroup_when_not_AutoPropagate_group_removed_then_count_of_triggers_in_AutoPropagate_question_should_be_the_same()
        {
            // Arrange
            var notAutoPropagateGroupId = Guid.NewGuid();
            AutoPropagateQuestion autoQuestion;
            var doc = this.CreateQuestionnaireDocumentWithRegularGroupAndAutoPropagateQuestion(notAutoPropagateGroupId, out autoQuestion);
            var expectedCountOfTriggers = autoQuestion.Triggers.Count;

            // Act
            doc.RemoveGroup(notAutoPropagateGroupId);

            // Assert
            Assert.That(autoQuestion.Triggers.Count, Is.EqualTo(expectedCountOfTriggers));
        }

        private QuestionnaireDocument CreateQuestionnaireDocumentWithRegularGroupAndAutoPropagateQuestion(Guid groupId, out AutoPropagateQuestion autoQuestion)
        {
            var doc = new QuestionnaireDocument();
            var chapter1 = new Group("Chapter 1") { PublicKey = Guid.NewGuid() };
            autoQuestion = new AutoPropagateQuestion("Auto question") { PublicKey = Guid.NewGuid(), MaxValue = 10, Triggers = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() } };
            var chapter2 = new Group("Chapter 2") { PublicKey = Guid.NewGuid() };
            var autoGroup = new Group("Auto Group") { PublicKey = groupId, Propagated = Propagate.None };

            chapter1.Children.Add(autoQuestion);
            chapter2.Children.Add(autoGroup);

            doc.Children.Add(chapter1);
            doc.Children.Add(chapter2);

            return doc;
        }

        private QuestionnaireDocument CreateQuestionnaireDocumentWithAutoPropagateGroupAndQuestion(Guid autoGroupId, out AutoPropagateQuestion autoQuestion)
        {
            var doc = new QuestionnaireDocument();
            var chapter1 = new Group("Chapter 1") { PublicKey = Guid.NewGuid() };
            autoQuestion = new AutoPropagateQuestion("Auto question") { PublicKey = Guid.NewGuid(), MaxValue = 10, Triggers = new List<Guid> { autoGroupId } };
            var chapter2 = new Group("Chapter 2") { PublicKey = Guid.NewGuid() };
            var autoGroup = new Group("Auto Group") { PublicKey = autoGroupId, Propagated = Propagate.AutoPropagated };

            chapter1.Children.Add(autoQuestion);
            chapter2.Children.Add(autoGroup);

            doc.Children.Add(chapter1);
            doc.Children.Add(chapter2);

            return doc;
        }
    }
}
