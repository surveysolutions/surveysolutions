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
            Questionnaire questionnaire = CreateQuestionnaire();

            // act
            TestDelegate act = () => questionnaire.NewAddGroup(Guid.NewGuid(), null, emptyTitle, Propagate.None, null, null);

            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.GroupTitleRequired));
        }

        [Test]
        public void NewAddGroup_When_groups_title_is_not_empty_Then_raised_NewAddGroup_event_contains_the_same_group_title()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                Questionnaire questionnaire = CreateQuestionnaire();
                string notEmptyNewTitle = "Some new title";

                // act
                questionnaire.NewAddGroup(Guid.NewGuid(), null, notEmptyNewTitle, Propagate.None, null, null);

                // assert
                Assert.That(GetSingleEvent<NewGroupAdded>(eventContext).GroupText, Is.EqualTo(notEmptyNewTitle));
            }
        }

        [Test]
        public void NewAddGroup_When_parent_group_has_AutoPropagate_propagation_kind_Then_throws_DomainException()
        {
            // arrange
            var parentAutoPropagateGroupId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            Questionnaire questionnaire = CreateQuestionnaireWithOneAutoPropagatedGroup(parentAutoPropagateGroupId);

            // act
            TestDelegate act = () => questionnaire.NewAddGroup(Guid.NewGuid(), parentAutoPropagateGroupId, "Title", Propagate.None, null, null);

            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.AutoPropagateGroupCantHaveChildGroups));
        }

        [Test]
        public void NewAddGroup_When_parent_group_is_non_propagated_Then_raised_NewAddGroup_event_contains_regular_group_id_as_parent()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                var parentRegularGroupId = Guid.Parse("11111111-1111-1111-1111-111111111111");
                Questionnaire questionnaire = CreateQuestionnaireWithOneNonPropagatedGroup(parentRegularGroupId);

                // act
                questionnaire.NewAddGroup(Guid.NewGuid(), parentRegularGroupId, "Title", Propagate.None, null, null);

                // assert
                Assert.That(GetLastEvent<NewGroupAdded>(eventContext).ParentGroupPublicKey, Is.EqualTo(parentRegularGroupId));
            }
        }

        [Test]
        public void NewAddGroup_When_groups_propagation_kind_is_unsupported_Then_throws_DomainException()
        {
            // arrange
            Questionnaire questionnaire = CreateQuestionnaire();
            var unsupportedPropagationKing = Propagate.Propagated;

            // act
            TestDelegate act = () => questionnaire.NewAddGroup(Guid.NewGuid(), null, "Title", unsupportedPropagationKing, null, null);

            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.NotSupportedPropagationGroup));
        }

        [TestCase(Propagate.None)]
        [TestCase(Propagate.AutoPropagated)]
        public void NewAddGroup_When_groups_propagation_kind_is_supported_Then_raised_NewAddGroup_event_contains_the_same_propagation_kind(Propagate supportedPopagationKind)
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                Questionnaire questionnaire = CreateQuestionnaire();

                // act
                questionnaire.NewAddGroup(Guid.NewGuid(), null, "Title", supportedPopagationKind, null, null);

                // assert
                Assert.That(GetSingleEvent<NewGroupAdded>(eventContext).Paropagateble, Is.EqualTo(supportedPopagationKind));
            }
        }
    }
}