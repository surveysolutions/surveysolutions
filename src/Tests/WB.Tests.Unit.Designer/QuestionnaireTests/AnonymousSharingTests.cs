using System;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    [TestFixture]
    internal class AnonymousSharingTests : QuestionnaireTestsContext
    {
        [Test]
        public void should_allow_owner_to_manage_anonymous_sharing()
        {
            var ownerId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaire(responsibleId: ownerId);

            Assert.DoesNotThrow(() => questionnaire.EnableAnonymousSharing(new EnableAnonymousSharing(questionnaire.Id, ownerId, false)));
            Assert.DoesNotThrow(() => questionnaire.RegenerateAnonymousSharingLink(new RegenerateAnonymousSharingLink(questionnaire.Id, ownerId, false)));
            Assert.DoesNotThrow(() => questionnaire.DisableAnonymousSharing(new DisableAnonymousSharing(questionnaire.Id, ownerId, false)));
        }

        [Test]
        public void should_allow_person_shared_with_edit_rights_to_manage_anonymous_sharing()
        {
            var ownerId = Guid.NewGuid();
            var editorId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaire(responsibleId: ownerId);
            questionnaire.AddSharedPerson(editorId, "editor@example.com", ShareType.Edit, ownerId);

            Assert.DoesNotThrow(() => questionnaire.EnableAnonymousSharing(new EnableAnonymousSharing(questionnaire.Id, editorId, false)));
        }

        [Test]
        public void should_allow_administrator_to_manage_anonymous_sharing()
        {
            Questionnaire questionnaire = CreateQuestionnaire(responsibleId: Guid.NewGuid());

            Assert.DoesNotThrow(() => questionnaire.EnableAnonymousSharing(new EnableAnonymousSharing(questionnaire.Id, Guid.NewGuid(), true)));
        }

        [Test]
        public void should_not_allow_person_without_access_to_manage_anonymous_sharing()
        {
            var ownerId = Guid.NewGuid();
            var strangerId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaire(responsibleId: ownerId);

            Assert.Throws<QuestionnaireException>(() => questionnaire.EnableAnonymousSharing(new EnableAnonymousSharing(questionnaire.Id, strangerId, false)));
            Assert.Throws<QuestionnaireException>(() => questionnaire.DisableAnonymousSharing(new DisableAnonymousSharing(questionnaire.Id, strangerId, false)));
            Assert.Throws<QuestionnaireException>(() => questionnaire.RegenerateAnonymousSharingLink(new RegenerateAnonymousSharingLink(questionnaire.Id, strangerId, false)));
        }

        [Test]
        public void should_not_allow_person_shared_with_view_rights_to_manage_anonymous_sharing()
        {
            var ownerId = Guid.NewGuid();
            var viewerId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaire(responsibleId: ownerId);
            questionnaire.AddSharedPerson(viewerId, "viewer@example.com", ShareType.View, ownerId);

            Assert.Throws<QuestionnaireException>(() => questionnaire.EnableAnonymousSharing(new EnableAnonymousSharing(questionnaire.Id, viewerId, false)));
        }
    }
}


