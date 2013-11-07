using System;
using Main.Core.Domain;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;

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
            Guid responsibleId = Guid.NewGuid();
            var questionnaire =
                CreateQuestionnaireWithChapterWithRegularAndAutoPropagateGroup(
                    autoPropagateGroupId: targetAutoPropagateGroupId, regularGroupId: groupId,
                    responsibleId: responsibleId);

            // Act
            TestDelegate act = () => questionnaire.MoveGroup(groupId, targetAutoPropagateGroupId, 0, responsibleId);

            // Assert
            var domainException = Assert.Throws<QuestionnaireException>(act);
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
                Guid responsibleId = Guid.NewGuid();
                var questionnaire =
                    CreateQuestionnaireWithChapterWithRegularAndAutoPropagateGroup(
                        autoPropagateGroupId: moveAutoPropagateGroupId, regularGroupId: targetRegularGroupId,
                        responsibleId: responsibleId);

                // Act
                questionnaire.MoveGroup(moveAutoPropagateGroupId, targetRegularGroupId, 0, responsibleId: responsibleId);

                // Assert
                Assert.That(GetSingleEvent<QuestionnaireItemMoved>(eventContext).PublicKey, Is.EqualTo(moveAutoPropagateGroupId));
            }
        }

        [Test]
        public void MoveGroup_When_User_Doesnot_Have_Permissions_For_Edit_Questionnaire_Then_DomainException_should_be_thrown()
        {
            // Arrange
            var moveAutoPropagateGroupId = Guid.NewGuid();
            var targetRegularGroupId = Guid.NewGuid();
            var questionnaire =
                CreateQuestionnaireWithChapterWithRegularAndAutoPropagateGroup(
                    autoPropagateGroupId: moveAutoPropagateGroupId, regularGroupId: targetRegularGroupId,
                    responsibleId: Guid.NewGuid());
            // act
            TestDelegate act = () => questionnaire.MoveGroup(moveAutoPropagateGroupId, targetRegularGroupId, 0, responsibleId: Guid.NewGuid());
            // assert
            var domainException = Assert.Throws<QuestionnaireException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.DoesNotHavePermissionsForEdit));
        }

        private Questionnaire CreateQuestionnaireWithChapterWithRegularAndAutoPropagateGroup(Guid autoPropagateGroupId, Guid regularGroupId, Guid responsibleId)
        {
            var chapterId = Guid.NewGuid();
            
            Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(questionnaireId: Guid.NewGuid(), groupId: chapterId, responsibleId: responsibleId);

            questionnaire.NewAddGroup(autoPropagateGroupId, chapterId, "autoPropagateGroup", Propagate.AutoPropagated, null, null, responsibleId: responsibleId);

            questionnaire.NewAddGroup(regularGroupId, chapterId, "regularGroup", Propagate.None, null, null, responsibleId: responsibleId);

            return questionnaire;
        }
    }
}
