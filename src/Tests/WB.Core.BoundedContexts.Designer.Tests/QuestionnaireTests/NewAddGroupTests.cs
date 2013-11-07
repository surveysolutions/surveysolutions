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
using WB.Core.BoundedContexts.Designer.Exceptions;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests
{
    [TestFixture]
    public class NewAddGroupTests : QuestionnaireTestsContext
    {
        [SetUp]
        public void SetUp()
        {
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);
        }

        [TestCase("")]
        [TestCase("   ")]
        [TestCase("\t")]
        public void NewAddGroup_When_groups_title_is_empty_or_whitespaces_Then_throws_DomainException(string emptyTitle)
        {
            // arrange
            Guid responsibleId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaire(responsibleId : responsibleId);

            // act
            TestDelegate act = () => questionnaire.NewAddGroup(Guid.NewGuid(), null, emptyTitle, Propagate.None, null, null, responsibleId: responsibleId);

            // assert
            var domainException = Assert.Throws<QuestionnaireException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.GroupTitleRequired));
        }

        [Test]
        public void NewAddGroup_When_groups_title_is_not_empty_Then_raised_NewAddGroup_event_contains_the_same_group_title()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                Guid responsibleId = Guid.NewGuid();
                Questionnaire questionnaire = CreateQuestionnaire(responsibleId : responsibleId);
                string notEmptyNewTitle = "Some new title";

                // act
                questionnaire.NewAddGroup(Guid.NewGuid(), null, notEmptyNewTitle, Propagate.None, null, null, responsibleId: responsibleId);

                // assert
                Assert.That(GetSingleEvent<NewGroupAdded>(eventContext).GroupText, Is.EqualTo(notEmptyNewTitle));
            }
        }

        [Test]
        public void NewAddGroup_When_parent_group_has_AutoPropagate_propagation_kind_Then_throws_DomainException()
        {
            // arrange
            var parentAutoPropagateGroupId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            Guid responsibleId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaireWithOneAutoPropagatedGroup(groupId: parentAutoPropagateGroupId, responsibleId: responsibleId);

            // act
            TestDelegate act = () => questionnaire.NewAddGroup(Guid.NewGuid(), parentAutoPropagateGroupId, "Title", Propagate.None, null, null, responsibleId: responsibleId);

            // assert
            var domainException = Assert.Throws<QuestionnaireException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.AutoPropagateGroupCantHaveChildGroups));
        }

        [Test]
        public void NewAddGroup_When_parent_group_is_non_propagated_Then_raised_NewAddGroup_event_contains_regular_group_id_as_parent()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                var parentRegularGroupId = Guid.Parse("11111111-1111-1111-1111-111111111111");
                Guid responsibleId = Guid.NewGuid();
                Questionnaire questionnaire = CreateQuestionnaireWithOneNonPropagatedGroup(groupId: parentRegularGroupId, responsibleId: responsibleId);

                // act
                questionnaire.NewAddGroup(Guid.NewGuid(), parentRegularGroupId, "Title", Propagate.None, null, null, responsibleId: responsibleId);

                // assert
                Assert.That(GetLastEvent<NewGroupAdded>(eventContext).ParentGroupPublicKey, Is.EqualTo(parentRegularGroupId));
            }
        }

        [Test]
        public void NewAddGroup_When_groups_propagation_kind_is_unsupported_Then_throws_DomainException()
        {
            // arrange
            Guid responsibleId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            var unsupportedPropagationKing = Propagate.Propagated;

            // act
            TestDelegate act = () => questionnaire.NewAddGroup(Guid.NewGuid(), null, "Title", unsupportedPropagationKing, null, null, responsibleId: responsibleId);

            // assert
            var domainException = Assert.Throws<QuestionnaireException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.NotSupportedPropagationGroup));
        }

        [TestCase(Propagate.None)]
        [TestCase(Propagate.AutoPropagated)]
        public void NewAddGroup_When_groups_propagation_kind_is_supported_Then_raised_NewAddGroup_event_contains_the_same_propagation_kind(Propagate supportedPopagationKind)
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                Guid responsibleId = Guid.NewGuid();
                Questionnaire questionnaire = CreateQuestionnaire(responsibleId: responsibleId);

                // act
                questionnaire.NewAddGroup(Guid.NewGuid(), null, "Title", supportedPopagationKind, null, null, responsibleId: responsibleId);

                // assert
                Assert.That(GetSingleEvent<NewGroupAdded>(eventContext).Paropagateble, Is.EqualTo(supportedPopagationKind));
            }
        }

        [TestCase(Propagate.None)]
        [TestCase(Propagate.AutoPropagated)]
        public void NewAddGroup_When_User_Doesnot_Have_Permissions_For_Edit_Questionnaire_Then_DomainException_should_be_thrown(Propagate supportedPopagationKind)
        {
            // arrange
            Questionnaire questionnaire = CreateQuestionnaire(responsibleId: Guid.NewGuid());

            // act
            TestDelegate act = () => questionnaire.NewAddGroup(Guid.NewGuid(), null, "Title", supportedPopagationKind, null, null, responsibleId: Guid.NewGuid());
            // assert
            var domainException = Assert.Throws<QuestionnaireException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.DoesNotHavePermissionsForEdit));
        }
    }
}