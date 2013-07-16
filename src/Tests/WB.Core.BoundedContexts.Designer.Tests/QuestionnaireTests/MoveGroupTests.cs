using System;
using Main.Core.Domain;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Aggregates;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests
{
    [TestFixture]
    public class MoveGroupTests : QuestionnaireTestsContext
    {
        [SetUp]
        public void SetUp()
        {
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);
        }

        [Test]
        public void MoveGroup_When_target_group_is_auto_propagateble_Then_throws_DomainException_with_type_AutoPropagateGroupCantHaveChildGroups()
        {
            // Arrange
            var groupId = Guid.NewGuid();
            var targetAutoPropagateGroupId = Guid.NewGuid();
            var questionnaire = CreateQuestionnaireWithChapterWithRegularAndAutoPropagateGroup(targetAutoPropagateGroupId, groupId);

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
                var questionnaire = CreateQuestionnaireWithChapterWithRegularAndAutoPropagateGroup(moveAutoPropagateGroupId, targetRegularGroupId);

                // Act
                questionnaire.MoveGroup(moveAutoPropagateGroupId, targetRegularGroupId, 0);

                // Assert
                Assert.That(GetSingleEvent<QuestionnaireItemMoved>(eventContext).PublicKey, Is.EqualTo(moveAutoPropagateGroupId));
            }
        }

        private Questionnaire CreateQuestionnaireWithChapterWithRegularAndAutoPropagateGroup(Guid autoPropagateGroupId, Guid regularGroupId)
        {
            var chapterId = Guid.NewGuid();
            
            Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(Guid.NewGuid(), chapterId);

            questionnaire.NewAddGroup(autoPropagateGroupId, chapterId, "autoPropagateGroup", Propagate.AutoPropagated, null, null);

            questionnaire.NewAddGroup(regularGroupId, chapterId, "regularGroup", Propagate.None, null, null);

            return questionnaire;
        }
    }
}
