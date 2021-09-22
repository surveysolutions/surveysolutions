using System;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Aggregates;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    [TestFixture]
    internal class MoveGroupTests : QuestionnaireTestsContext
    {
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
        
        [Test]
        public void MoveGroup_When_User_try_move_subsection_with_not_allowed_entities_into_cover_Then_DomainException_should_be_thrown()
        {
            // Arrange
            var groupId = Guid.NewGuid();
            Guid responsibleId = Guid.NewGuid();
            var questionnaire =
                CreateQuestionnaireWithOneGroup(
                    groupId: groupId,
                    responsibleId: responsibleId);
            questionnaire.AddGroup(Guid.NewGuid(), groupId, responsibleId);
            
            // act
            TestDelegate act = () => 
                questionnaire.MoveGroup(groupId, questionnaire.QuestionnaireDocument.CoverPageSectionId, 0, responsibleId: responsibleId);

            // assert
            var domainException = Assert.Throws<QuestionnaireException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.CanNotAddElementToCoverPage));
        }

        [Test]
        public void MoveGroup_When_User_try_move_subsection_with_only_allowed_entities_into_cover_Then_content_of_section_should_be_moved()
        {
            // Arrange
            var groupId = Guid.NewGuid();
            var staticTextId = Guid.NewGuid();
            var questionId = Guid.NewGuid();
            Guid responsibleId = Guid.NewGuid();
            
            var questionnaire = CreateQuestionnaireWithOneGroup(
                    groupId: groupId,
                    responsibleId: responsibleId);
            questionnaire.AddTextQuestion(questionId, groupId, responsibleId);
            questionnaire.AddStaticTextAndMoveIfNeeded(Create.Command.AddStaticText(questionnaire.Id, staticTextId, "text", responsibleId, groupId));
            
            // act
            questionnaire.MoveGroup(groupId, questionnaire.QuestionnaireDocument.CoverPageSectionId, 0, responsibleId: responsibleId);

            // assert
            var question = questionnaire.QuestionnaireDocument.Find<IQuestion>(questionId);
            Assert.That(question!.Featured, Is.True);
            Assert.That(question!.GetParent()!.PublicKey, Is.EqualTo(questionnaire.QuestionnaireDocument.CoverPageSectionId));

            var staticText = questionnaire.QuestionnaireDocument.Find<IStaticText>(staticTextId);
            Assert.That(staticText!.GetParent()!.PublicKey, Is.EqualTo(questionnaire.QuestionnaireDocument.CoverPageSectionId));

            var group = questionnaire.QuestionnaireDocument.Find<IGroup>(groupId);
            Assert.That(group, Is.Not.Null);
            Assert.That(group.Children.Count, Is.EqualTo(0));
        }

        [Test]
        public void MoveQuestion_When_User_try_move_question_to_cover_Then_Featured_flag_is_true()
        {
            // Arrange
            var questionId = Guid.NewGuid();
            Guid responsibleId = Guid.NewGuid();
            var questionnaire =
                CreateQuestionnaireWithOneQuestion(
                    questionId: questionId,
                    responsibleId: responsibleId);
            
            // act
            questionnaire.MoveQuestion(questionId, questionnaire.QuestionnaireDocument.CoverPageSectionId, 0, responsibleId: responsibleId);

            // assert
            var question = questionnaire.QuestionnaireDocument.Find<IQuestion>(questionId);
            Assert.That(question!.Featured, Is.True);
            Assert.That(question!.GetParent()!.PublicKey, Is.EqualTo(questionnaire.QuestionnaireDocument.CoverPageSectionId));
        }

        [Test]
        public void MoveQuestion_When_User_try_move_question_from_cover_Then_Featured_flag_is_false()
        {
            // Arrange
            var questionId = Guid.NewGuid();
            var groupId = Guid.NewGuid();
            Guid responsibleId = Guid.NewGuid();
            var questionnaire =
                CreateQuestionnaireWithOneGroup(
                    groupId: groupId,
                    responsibleId: responsibleId);
            questionnaire.AddTextQuestion(questionId, questionnaire.QuestionnaireDocument.CoverPageSectionId, responsibleId);
            
            // act
            questionnaire.MoveQuestion(questionId, groupId, 0, responsibleId: responsibleId);

            // assert
            var question = questionnaire.QuestionnaireDocument.Find<IQuestion>(questionId);
            Assert.That(question!.Featured, Is.False);
            Assert.That(question!.GetParent()!.PublicKey, Is.EqualTo(groupId));
        }
        
        [Test]
        public void When_moving_group_having_question_and_static_text_with_enablement_to_cover_Then_both_Enabling_condition_are_empty()
        {
            // Arrange
            var groupId = Guid.NewGuid();
            var staticTextId = Guid.NewGuid();
            var questionId = Guid.NewGuid();
            Guid responsibleId = Guid.NewGuid();
            
            var questionnaire = CreateQuestionnaireWithOneGroup(
                groupId: groupId,
                responsibleId: responsibleId);
            questionnaire.AddTextQuestion(questionId, groupId, responsibleId, enablementCondition:"1==1");
            questionnaire.AddStaticText(staticTextId, groupId, responsibleId, "text","1==1");
            
            // act
            questionnaire.MoveGroup(groupId, questionnaire.QuestionnaireDocument.CoverPageSectionId, 0, responsibleId: responsibleId);

            // assert
            var question = questionnaire.QuestionnaireDocument.Find<IQuestion>(questionId);
            Assert.That(question!.ConditionExpression, Is.Empty);
            Assert.That(question!.GetParent()!.PublicKey, Is.EqualTo(questionnaire.QuestionnaireDocument.CoverPageSectionId));

            var staticText = questionnaire.QuestionnaireDocument.Find<IStaticText>(staticTextId);
            Assert.That(staticText!.ConditionExpression, Is.Empty);
            Assert.That(staticText!.GetParent()!.PublicKey, Is.EqualTo(questionnaire.QuestionnaireDocument.CoverPageSectionId));
        }
    }
}
