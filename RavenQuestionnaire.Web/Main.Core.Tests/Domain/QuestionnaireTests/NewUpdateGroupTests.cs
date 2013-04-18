using System;
using Main.Core.Domain;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using NUnit.Framework;
using Ncqrs.Spec;

namespace Main.Core.Tests.Domain.QuestionnaireTests
{
    [TestFixture]
    public class NewUpdateGroupTests : QuestionnaireARTestContext
    {

        [Test]
        public void NewUpdateGroup_When_new_propagation_kind_is_None_Then_raised_GroupUpdated_event_with_propagation_kind_None()
        {
            using (var eventContext = new EventContext())
            {
                // Arrange
                Guid groupId = Guid.Parse("11111111-1111-1111-1111-111111111111");
                var newPropagationKind = Propagate.None;
                QuestionnaireAR questionnaire = CreateQuestionnaireARWithRegularGroupAndRegularGroupInIt(groupId);

                // Act
                questionnaire.NewUpdateGroup(groupId, "New title", newPropagationKind, null, null);

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
                QuestionnaireAR questionnaire = QuestionnaireARTestContext.CreateQuestionnaireARWithOneGroupAndQuestionInIt(Guid.NewGuid(), groupId, Propagate.None);

                // act
                questionnaire.NewUpdateGroup(groupId, "New title", newPropagationKind, null, null);

                // assert
                Assert.That(QuestionnaireARTestContext.GetSingleEvent<GroupUpdated>(eventContext).Propagateble, Is.EqualTo(newPropagationKind));
            }
        }

        [Test]
        public void NewUpdateGroup_When_new_propagation_kind_of_group_with_subgroups_is_AutoPropagate_Then_throws_DomainException_with_type_GroupCantBecomeAutoPropagateIfHasAnyChildGroup()
        {
            // arrange
            Guid groupId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var newPropagationKind = Propagate.AutoPropagated;
            QuestionnaireAR questionnaire = CreateQuestionnaireARWithRegularGroupAndRegularGroupInIt(groupId);

            // act
            TestDelegate act = () => questionnaire.NewUpdateGroup(groupId, "New title", newPropagationKind, null, null);

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
            QuestionnaireAR questionnaire = QuestionnaireARTestContext.CreateQuestionnaireARWithOneGroup(Guid.NewGuid(), groupPublicKey);

            // act
            TestDelegate act = () => questionnaire.NewUpdateGroup(groupPublicKey, emptyTitle, Propagate.None, null, null);

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
                QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneGroup(Guid.NewGuid(), groupPublicKey);
                string notEmptyNewTitle = "Some new title";

                // act
                questionnaire.NewUpdateGroup(groupPublicKey, notEmptyNewTitle, Propagate.None, null, null);

                // assert
                Assert.That(QuestionnaireARTestContext.GetSingleEvent<GroupUpdated>(eventContext).GroupText, Is.EqualTo(notEmptyNewTitle));
            }
        }

        [Test]
        public void NewUpdateGroup_When_groups_propagation_kind_is_unsupported_Then_throws_DomainException()
        {
            // arrange
            var groupPublicKey = Guid.NewGuid();
            QuestionnaireAR questionnaire = QuestionnaireARTestContext.CreateQuestionnaireARWithOneGroup(Guid.NewGuid(), groupPublicKey);
            var unsupportedPropagationKing = Propagate.Propagated;

            // act
            TestDelegate act = () => questionnaire.NewUpdateGroup(groupPublicKey, "Title", unsupportedPropagationKing, null, null);

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
                QuestionnaireAR questionnaire = QuestionnaireARTestContext.CreateQuestionnaireARWithOneGroup(Guid.NewGuid(), groupPublicKey);

                // act
                questionnaire.NewUpdateGroup(groupPublicKey, "Title", supportedPopagationKind, null, null);

                // assert
                Assert.That(QuestionnaireARTestContext.GetSingleEvent<GroupUpdated>(eventContext).Propagateble, Is.EqualTo(supportedPopagationKind));
            }
        }

        [Test]
        public void NewUpdateGroup_When_group_does_not_exist_Then_throws_DomainException()
        {
            // arrange
            QuestionnaireAR questionnaire = QuestionnaireARTestContext.CreateQuestionnaireAR();
            Guid notExistingGroupPublicKey = Guid.NewGuid();

            // act
            TestDelegate act = () =>
                {
                    questionnaire.NewUpdateGroup(notExistingGroupPublicKey, null, Propagate.None, null, null);
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
                QuestionnaireAR questionnaire = QuestionnaireARTestContext.CreateQuestionnaireARWithOneGroup(questionnaireId, existingGroupPublicKey);

                // act
                questionnaire.NewUpdateGroup(existingGroupPublicKey, "Title", Propagate.None, null, null);

                // assert
                Assert.That(QuestionnaireARTestContext.GetSingleEvent<GroupUpdated>(eventContext).QuestionnaireId, Is.EqualTo(questionnaireId.ToString()));
            }
        }

        [Test]
        public void NewUpdateGroup_When_group_exists_Then_raised_GroupUpdated_event_contains_group_public_key()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                var groupPublicKey = Guid.NewGuid();
                QuestionnaireAR questionnaire = QuestionnaireARTestContext.CreateQuestionnaireARWithOneGroup(groupId: groupPublicKey);

                // act
                questionnaire.NewUpdateGroup(groupPublicKey, "group text", Propagate.None, null, null);

                // assert
                Assert.That(QuestionnaireARTestContext.GetSingleEvent<GroupUpdated>(eventContext).GroupPublicKey, Is.EqualTo(groupPublicKey));
            }
        }

        [Test]
        public void NewUpdateGroup_When_group_exists_and_group_text_specified_Then_raised_GroupUpdated_event_with_same_group_text()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                var groupPublicKey = Guid.NewGuid();
                QuestionnaireAR questionnaire = QuestionnaireARTestContext.CreateQuestionnaireARWithOneGroup(groupId: groupPublicKey);
                var groupText = "new group text";

                // act
                questionnaire.NewUpdateGroup(groupPublicKey, groupText, Propagate.None, null, null);

                // assert
                Assert.That(QuestionnaireARTestContext.GetSingleEvent<GroupUpdated>(eventContext).GroupText, Is.EqualTo(groupText));
            }
        }

        [Test]
        public void NewUpdateGroup_When_group_exists_and_propogatability_specified_Then_raised_GroupUpdated_event_with_same_propogatability()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                var groupPublicKey = Guid.NewGuid();
                QuestionnaireAR questionnaire = QuestionnaireARTestContext.CreateQuestionnaireARWithOneGroup(groupId: groupPublicKey);
                var propagatability = Propagate.AutoPropagated;

                // act
                questionnaire.NewUpdateGroup(groupPublicKey, "new text", propagatability, null, null);

                // assert
                Assert.That(QuestionnaireARTestContext.GetSingleEvent<GroupUpdated>(eventContext).Propagateble, Is.EqualTo(propagatability));
            }
        }

        [Test]
        public void NewUpdateGroup_When_group_exists_and_condition_expression_specified_Then_raised_GroupUpdated_event_with_same_condition_expression()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                var groupPublicKey = Guid.NewGuid();
                QuestionnaireAR questionnaire = QuestionnaireARTestContext.CreateQuestionnaireARWithOneGroup(groupId: groupPublicKey);
                var conditionExpression = "2 < 7";

                // act
                questionnaire.NewUpdateGroup(groupPublicKey, "text of a group", Propagate.None, null, conditionExpression);

                // assert
                Assert.That(QuestionnaireARTestContext.GetSingleEvent<GroupUpdated>(eventContext).ConditionExpression, Is.EqualTo(conditionExpression));
            }
        }

        [Test]
        public void NewUpdateGroup_When_group_exists_and_description_specified_Then_raised_GroupUpdated_event_with_same_description()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                var groupPublicKey = Guid.NewGuid();
                QuestionnaireAR questionnaire = QuestionnaireARTestContext.CreateQuestionnaireARWithOneGroup(groupId: groupPublicKey);
                var description = "hardest questionnaire in the world";

                // act
                questionnaire.NewUpdateGroup(groupPublicKey, "Title", Propagate.None, description, null);

                // assert
                Assert.That(QuestionnaireARTestContext.GetSingleEvent<GroupUpdated>(eventContext).Description, Is.EqualTo(description));
            }
        }
    }
}