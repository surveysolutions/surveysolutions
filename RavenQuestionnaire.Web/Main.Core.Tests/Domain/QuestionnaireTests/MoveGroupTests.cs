using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Main.Core.Domain;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using NUnit.Framework;
using Ncqrs.Spec;

namespace Main.Core.Tests.Domain.QuestionnaireTests
{
    [TestFixture]
    public class MoveGroupTests : QuestionnaireARTestContext
    {
        [Test]
        public void MoveGroup_When_target_group_is_auto_propagateble_Then_throws_DomainException_with_type_AutoPropagateGroupCantHaveChildGroups()
        {
            // Arrange
            var groupId = Guid.NewGuid();
            var targetAutoPropagateGroupId = Guid.NewGuid();
            var questionnaire = CreateQuestionnaireARWithChapterWithRegularAndAutoPropagateGroup(targetAutoPropagateGroupId, groupId);

            // Act
            TestDelegate act = () => questionnaire.MoveGroup(groupId, targetAutoPropagateGroupId, 0);

            // Assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.AutoPropagateGroupCantHaveChildGroups));
        }

        [Test]
        public void MoveGroup_When_target_group_is_regular_Then_rised_QuestionnaireItemMoved_event_s()
        {
            using (var eventContext = new EventContext())
            {
                // Arrange
                var moveAutoPropagateGroupId = Guid.NewGuid();
                var targetRegularGroupId = Guid.NewGuid();
                var questionnaire = CreateQuestionnaireARWithChapterWithRegularAndAutoPropagateGroup(moveAutoPropagateGroupId, targetRegularGroupId);

                // Act
                questionnaire.MoveGroup(moveAutoPropagateGroupId, targetRegularGroupId, 0);

                // Assert
                Assert.That(GetSingleEvent<QuestionnaireItemMoved>(eventContext).PublicKey, Is.EqualTo(moveAutoPropagateGroupId));
            }
        }

        private QuestionnaireAR CreateQuestionnaireARWithChapterWithRegularAndAutoPropagateGroup(Guid autoPropagateGroupId, Guid regularGroupId)
        {
            var chapterId = Guid.NewGuid();
            
            QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneGroup(Guid.NewGuid(), chapterId);

            questionnaire.NewAddGroup(autoPropagateGroupId, chapterId, "autoPropagateGroup", Propagate.AutoPropagated, null, null);

            questionnaire.NewAddGroup(regularGroupId, chapterId, "regularGroup", Propagate.None, null, null);

            return questionnaire;
        }
    }
}
