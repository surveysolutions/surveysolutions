﻿using System;
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
        public void MoveGroup_When_target_group_is_regular_Then_rised_QuestionnaireItemMoved_event_s()
        {
            using (var eventContext = new EventContext())
            {
                // Arrange
                var moveAutoPropagateGroupId = Guid.NewGuid();
                var targetRegularGroupId = Guid.NewGuid();
                Guid responsibleId = Guid.NewGuid();
                var questionnaire =
                    this.CreateQuestionnaireWithChapterWithRegularAndRosterGroup(
                        rosterGroupId: moveAutoPropagateGroupId, regularGroupId: targetRegularGroupId,
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
                this.CreateQuestionnaireWithChapterWithRegularAndRosterGroup(
                    rosterGroupId: moveAutoPropagateGroupId, regularGroupId: targetRegularGroupId,
                    responsibleId: Guid.NewGuid());
            // act
            TestDelegate act = () => questionnaire.MoveGroup(moveAutoPropagateGroupId, targetRegularGroupId, 0, responsibleId: Guid.NewGuid());
            // assert
            var domainException = Assert.Throws<QuestionnaireException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.DoesNotHavePermissionsForEdit));
        }

        private Questionnaire CreateQuestionnaireWithChapterWithRegularAndRosterGroup(Guid rosterGroupId, Guid regularGroupId, Guid responsibleId)
        {
            var chapterId = Guid.NewGuid();
            
            Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(questionnaireId: Guid.NewGuid(), groupId: chapterId, responsibleId: responsibleId);

            Guid rosterSizeQuestionId = Guid.NewGuid();
            questionnaire.AddGroup(regularGroupId, responsibleId: responsibleId, title: "regularGroup", variableName: null, rosterSizeQuestionId: null,
                description: null, condition: null, parentGroupId: chapterId, isRoster: false,
                rosterSizeSource: RosterSizeSourceType.Question, rosterFixedTitles: null, rosterTitleQuestionId: null);
            questionnaire.AddMultiOptionQuestion(rosterSizeQuestionId, regularGroupId, "rosterSizeQuestion", 
                "rosterSizeQuestion",null, false, QuestionScope.Interviewer, "", "", "", "", responsibleId,
                new[] { new Option(Guid.NewGuid(), "1", "opt1"), new Option(Guid.NewGuid(), "2", "opt2") },  null,
                false, null);
            questionnaire.AddGroup(rosterGroupId, responsibleId: responsibleId, title: "autoPropagateGroup", variableName: null,
                rosterSizeQuestionId: rosterSizeQuestionId, description: null, condition: null, parentGroupId: chapterId, isRoster: true,
                rosterSizeSource: RosterSizeSourceType.Question, rosterFixedTitles: null, rosterTitleQuestionId: null);
            

            return questionnaire;
        }
    }
}
