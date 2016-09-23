using System;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Exceptions;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    [TestFixture]
    internal class MoveGroupTests : QuestionnaireTestsContext
    {
        [SetUp]
        public void SetUp()
        {
            AssemblyContext.SetupServiceLocator();
        }

        [Test]
        public void MoveGroup_When_target_group_is_regular_Then_rised_QuestionnaireItemMoved_event_s()
        {
            // Arrange
            var moveAutoPropagateGroupId = Guid.NewGuid();
            var targetRegularGroupId = Guid.NewGuid();
            Guid responsibleId = Guid.NewGuid();
            var questionnaire =
                CreateQuestionnaireWithChapterWithRegularAndRosterGroup(
                    rosterGroupId: moveAutoPropagateGroupId,
                    regularGroupId: targetRegularGroupId,
                    responsibleId: responsibleId);

            // Act
            questionnaire.MoveGroup(moveAutoPropagateGroupId, targetRegularGroupId, 0, responsibleId: responsibleId);

            // Assert
            Assert.That(questionnaire.QuestionnaireDocument.Find<IGroup>(moveAutoPropagateGroupId).PublicKey, Is.EqualTo(moveAutoPropagateGroupId));
        }

        [Test]
        public void MoveGroup_When_User_Doesnot_Have_Permissions_For_Edit_Questionnaire_Then_DomainException_should_be_thrown()
        {
            // Arrange
            var moveAutoPropagateGroupId = Guid.NewGuid();
            var targetRegularGroupId = Guid.NewGuid();
            var questionnaire =
                CreateQuestionnaireWithChapterWithRegularAndRosterGroup(
                    rosterGroupId: moveAutoPropagateGroupId,
                    regularGroupId: targetRegularGroupId,
                    responsibleId: Guid.NewGuid());
            // act
            TestDelegate act = () => questionnaire.MoveGroup(moveAutoPropagateGroupId, targetRegularGroupId, 0, responsibleId: Guid.NewGuid());

            // assert
            var domainException = Assert.Throws<QuestionnaireException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.DoesNotHavePermissionsForEdit));
        }
    }
}
