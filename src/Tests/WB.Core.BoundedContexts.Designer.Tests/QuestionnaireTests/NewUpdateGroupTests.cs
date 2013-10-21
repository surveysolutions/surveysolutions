using System;
using Main.Core.Domain;
using Main.Core.Domain.Exceptions;
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
    public class NewUpdateGroupTests : QuestionnaireTestsContext
    {
        [SetUp]
        public void SetUp()
        {
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);
        }

        [Test]
        public void NewUpdateGroup_When_new_propagation_kind_is_None_Then_raised_GroupUpdated_event_with_propagation_kind_None()
        {
            using (var eventContext = new EventContext())
            {
                // Arrange
                Guid groupId = Guid.Parse("11111111-1111-1111-1111-111111111111");
                var newPropagationKind = Propagate.None;
                Guid responsibleId = Guid.NewGuid();
                Questionnaire questionnaire = CreateQuestionnaireWithRegularGroupAndRegularGroupInIt(groupId: groupId, responsibleId: responsibleId);

                // Act
                questionnaire.NewUpdateGroup(groupId, "New title", newPropagationKind, null, null, responsibleId: responsibleId);

                // Assert
                Assert.That(GetSingleEvent<GroupUpdated>(eventContext).Propagateble, Is.EqualTo(newPropagationKind));
            }
        }

        [Test]
        public void NewUpdateGroup_When_new_propagation_kind_of_group_without_subgroups_is_AutoPropagate_Then_throws_DomainException_with_type_GroupCantBecomeAutoPropagateIfHasAnyChildGroup()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                var groupId = Guid.Parse("11111111-1111-1111-1111-111111111111");
                var newPropagationKind = Propagate.AutoPropagated;
                Guid responsibleId = Guid.NewGuid();
                Questionnaire questionnaire = CreateQuestionnaireWithOneGroupAndQuestionInIt(
                    questionId: Guid.NewGuid(), groupId: groupId, groupPropagationKind: Propagate.None,
                    responsibleId: responsibleId);

                // act
                questionnaire.NewUpdateGroup(groupId, "New title", newPropagationKind, null, null, responsibleId: responsibleId);

                // assert
                Assert.That(GetSingleEvent<GroupUpdated>(eventContext).Propagateble, Is.EqualTo(newPropagationKind));
            }
        }

        [Test]
        public void NewUpdateGroup_When_new_propagation_kind_of_group_with_subgroups_is_AutoPropagate_Then_throws_DomainException_with_type_GroupCantBecomeAutoPropagateIfHasAnyChildGroup()
        {
            // arrange
            Guid groupId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var newPropagationKind = Propagate.AutoPropagated;
            Guid responsibleId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaireWithRegularGroupAndRegularGroupInIt(groupId: groupId, responsibleId: responsibleId);

            // act
            TestDelegate act = () => questionnaire.NewUpdateGroup(groupId, "New title", newPropagationKind, null, null, responsibleId: responsibleId);

            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.GroupCantBecomeAutoPropagateIfHasAnyChildGroup));
        }

        [TestCase("")]
        [TestCase("   ")]
        [TestCase("\t")]
        public void NewUpdateGroup_When_groups_new_title_is_empty_or_whitespaces_Then_throws_DomainException(string emptyTitle)
        {
            // arrange
            var groupPublicKey = Guid.NewGuid();
            Guid responsibleId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(questionnaireId: Guid.NewGuid(), groupId: groupPublicKey, responsibleId: responsibleId);

            // act
            TestDelegate act = () => questionnaire.NewUpdateGroup(groupPublicKey, emptyTitle, Propagate.None, null, null, responsibleId: responsibleId);

            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.GroupTitleRequired));
        }

        [Test]
        public void NewUpdateGroup_When_groups_new_title_is_not_empty_Then_raised_GroupUpdated_event_contains_the_same_group_title()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                var groupPublicKey = Guid.NewGuid();
                Guid responsibleId = Guid.NewGuid();
                Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(questionnaireId: Guid.NewGuid(), groupId: groupPublicKey, responsibleId: responsibleId);
                string notEmptyNewTitle = "Some new title";

                // act
                questionnaire.NewUpdateGroup(groupPublicKey, notEmptyNewTitle, Propagate.None, null, null, responsibleId: responsibleId);

                // assert
                Assert.That(GetSingleEvent<GroupUpdated>(eventContext).GroupText, Is.EqualTo(notEmptyNewTitle));
            }
        }

        [Test]
        public void NewUpdateGroup_When_groups_propagation_kind_is_unsupported_Then_throws_DomainException()
        {
            // arrange
            var groupPublicKey = Guid.NewGuid();
            Guid responsibleId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(questionnaireId: Guid.NewGuid(), groupId: groupPublicKey, responsibleId: responsibleId);
            var unsupportedPropagationKing = Propagate.Propagated;

            // act
            TestDelegate act = () => questionnaire.NewUpdateGroup(groupPublicKey, "Title", unsupportedPropagationKing, null, null, responsibleId: responsibleId);

            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.NotSupportedPropagationGroup));
        }

        [TestCase(Propagate.None)]
        [TestCase(Propagate.AutoPropagated)]
        public void NewUpdateGroup_When_groups_propagation_kind_is_supported_Then_raised_GroupUpdated_event_contains_the_same_propagation_kind(Propagate supportedPopagationKind)
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                var groupPublicKey = Guid.NewGuid();
                Guid responsibleId = Guid.NewGuid();
                Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(questionnaireId: Guid.NewGuid(), groupId: groupPublicKey, responsibleId: responsibleId);

                // act
                questionnaire.NewUpdateGroup(groupPublicKey, "Title", supportedPopagationKind, null, null, responsibleId: responsibleId);

                // assert
                Assert.That(GetSingleEvent<GroupUpdated>(eventContext).Propagateble, Is.EqualTo(supportedPopagationKind));
            }
        }

        [Test]
        public void NewUpdateGroup_When_group_does_not_exist_Then_throws_DomainException()
        {
            // arrange
            Guid responsibleId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            Guid notExistingGroupPublicKey = Guid.NewGuid();

            // act
            TestDelegate act = () =>
                {
                    questionnaire.NewUpdateGroup(notExistingGroupPublicKey, null, Propagate.None, null, null, responsibleId: responsibleId);
                };

            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.GroupNotFound));
        }

        [Test]
        public void NewUpdateGroup_When_group_exists_Then_raised_GroupUpdated_event_contains_questionnaire_id()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                var questionnaireId = Guid.NewGuid();
                var existingGroupPublicKey = Guid.NewGuid();
                Guid responsibleId = Guid.NewGuid();
                Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(questionnaireId: questionnaireId, groupId: existingGroupPublicKey, responsibleId: responsibleId);

                // act
                questionnaire.NewUpdateGroup(existingGroupPublicKey, "Title", Propagate.None, null, null, responsibleId: responsibleId);

                // assert
                Assert.That(GetSingleEvent<GroupUpdated>(eventContext).QuestionnaireId, Is.EqualTo(questionnaireId.ToString()));
            }
        }

        [Test]
        public void NewUpdateGroup_When_group_exists_Then_raised_GroupUpdated_event_contains_group_public_key()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                var groupPublicKey = Guid.NewGuid();
                Guid responsibleId = Guid.NewGuid();
                Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(groupId: groupPublicKey, responsibleId: responsibleId);

                // act
                questionnaire.NewUpdateGroup(groupPublicKey, "group text", Propagate.None, null, null, responsibleId: responsibleId);

                // assert
                Assert.That(GetSingleEvent<GroupUpdated>(eventContext).GroupPublicKey, Is.EqualTo(groupPublicKey));
            }
        }

        [Test]
        public void NewUpdateGroup_When_group_exists_and_group_text_specified_Then_raised_GroupUpdated_event_with_same_group_text()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                var groupPublicKey = Guid.NewGuid();
                Guid responsibleId = Guid.NewGuid();
                Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(groupId: groupPublicKey, responsibleId: responsibleId);
                var groupText = "new group text";

                // act
                questionnaire.NewUpdateGroup(groupPublicKey, groupText, Propagate.None, null, null, responsibleId: responsibleId);

                // assert
                Assert.That(GetSingleEvent<GroupUpdated>(eventContext).GroupText, Is.EqualTo(groupText));
            }
        }

        [Test]
        public void NewUpdateGroup_When_group_exists_and_propogatability_specified_Then_raised_GroupUpdated_event_with_same_propogatability()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                var groupPublicKey = Guid.NewGuid();
                Guid responsibleId = Guid.NewGuid();
                Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(groupId: groupPublicKey, responsibleId: responsibleId);
                var propagatability = Propagate.AutoPropagated;

                // act
                questionnaire.NewUpdateGroup(groupPublicKey, "new text", propagatability, null, null, responsibleId: responsibleId);

                // assert
                Assert.That(GetSingleEvent<GroupUpdated>(eventContext).Propagateble, Is.EqualTo(propagatability));
            }
        }

        [Test]
        public void NewUpdateGroup_When_group_exists_and_condition_expression_specified_Then_raised_GroupUpdated_event_with_same_condition_expression()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                var groupPublicKey = Guid.NewGuid();
                Guid responsibleId = Guid.NewGuid();
                Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(groupId: groupPublicKey, responsibleId: responsibleId);
                var conditionExpression = "2 < 7";

                // act
                questionnaire.NewUpdateGroup(groupPublicKey, "text of a group", Propagate.None, null, conditionExpression, responsibleId: responsibleId);

                // assert
                Assert.That(GetSingleEvent<GroupUpdated>(eventContext).ConditionExpression, Is.EqualTo(conditionExpression));
            }
        }

        [Test]
        public void NewUpdateGroup_When_group_exists_and_description_specified_Then_raised_GroupUpdated_event_with_same_description()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                var groupPublicKey = Guid.NewGuid();
                Guid responsibleId = Guid.NewGuid();
                Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(groupId: groupPublicKey, responsibleId: responsibleId);
                var description = "hardest questionnaire in the world";

                // act
                questionnaire.NewUpdateGroup(groupPublicKey, "Title", Propagate.None, description, null, responsibleId: responsibleId);

                // assert
                Assert.That(GetSingleEvent<GroupUpdated>(eventContext).Description, Is.EqualTo(description));
            }
        }

        [Test]
        public void NewUpdateGroup_When_User_Doesnot_Have_Permissions_For_Edit_Questionnaire_Then_DomainException_should_be_thrown()
        {
            // arrange
            var groupPublicKey = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(groupId: groupPublicKey, responsibleId: Guid.NewGuid());
            var description = "hardest questionnaire in the world";

            // act
            TestDelegate act = () => questionnaire.NewUpdateGroup(groupPublicKey, "Title", Propagate.None, description, null, responsibleId: Guid.NewGuid());
            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.DoesNotHavePermissionsForEdit));
        }
    }
}