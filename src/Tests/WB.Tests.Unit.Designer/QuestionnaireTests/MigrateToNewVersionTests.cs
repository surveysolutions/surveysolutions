using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.QuestionnaireTests
{
    [TestFixture]
    internal class MigrateToNewVersionTests : QuestionnaireTestsContext
    {
        [Test]
        public void when_run_migrate_then_should_add_new_cover_support()
        {
            // arrange
            Guid coverId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaire(responsibleId: Guid.NewGuid());
            // it is need to get questionnaire in old format
            questionnaire.QuestionnaireDocument.CoverPageSectionId = coverId;

            // act
            questionnaire.MigrateToNewVersion();

            // assert
            var cover = questionnaire.QuestionnaireDocument.Find<IGroup>(coverId);
            Assert.That(cover, Is.Not.Null);
            Assert.That(cover.Title, Is.EqualTo(QuestionnaireEditor.CoverPageSection));
            Assert.That(questionnaire.QuestionnaireDocument.Children.First(), Is.EqualTo(cover));
        }

        [Test]
        public void when_run_migrate_on_new_version_should_receive_exception()
        {
            // arrange
            Questionnaire questionnaire = CreateQuestionnaire(responsibleId: Guid.NewGuid());

            // act
            var exception = Assert.Catch(() => questionnaire.MigrateToNewVersion());

            // assert
            Assert.That(exception, Is.Not.Null);
            Assert.That(exception.GetType(), Is.EqualTo(typeof(QuestionnaireException)));
        }
    }
}