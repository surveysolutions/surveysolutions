using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Tests.Abc;
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
            var responsibleId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            // it is need to get questionnaire in old format
            questionnaire.QuestionnaireDocument.CoverPageSectionId = coverId;
            questionnaire.AddTextQuestion(Id.g1, questionnaire.QuestionnaireDocument.Children[1].PublicKey, responsibleId, 
                isPreFilled: true);
            questionnaire.AddTextQuestion(Id.g2, questionnaire.QuestionnaireDocument.Children[1].PublicKey, responsibleId, 
                isPreFilled: false);
            questionnaire.AddTextQuestion(Id.g3, questionnaire.QuestionnaireDocument.Children[1].PublicKey, responsibleId, 
                isPreFilled: true);

            // act
            questionnaire.MigrateToNewVersion();

            // assert
            var cover = questionnaire.QuestionnaireDocument.Find<IGroup>(coverId);
            Assert.That(cover, Is.Not.Null);
            Assert.That(cover.Title, Is.EqualTo(QuestionnaireEditor.CoverPageSection));
            Assert.That(questionnaire.QuestionnaireDocument.Children.First(), Is.EqualTo(cover));

            var question1 = questionnaire.QuestionnaireDocument.Find<IQuestion>(Id.g1);
            Assert.That(question1, Is.Not.Null);
            Assert.That(question1.GetParent().PublicKey, Is.EqualTo(questionnaire.QuestionnaireDocument.CoverPageSectionId));
            Assert.That(question1.Featured, Is.True);
            Assert.That(cover.Children.First(), Is.EqualTo(question1));

            var question2 = questionnaire.QuestionnaireDocument.Find<IQuestion>(Id.g2);
            Assert.That(question2, Is.Not.Null);
            Assert.That(question2.GetParent().PublicKey, Is.Not.EqualTo(questionnaire.QuestionnaireDocument.CoverPageSectionId));
            Assert.That(question2.Featured, Is.False);

            var question3 = questionnaire.QuestionnaireDocument.Find<IQuestion>(Id.g3);
            Assert.That(question3, Is.Not.Null);
            Assert.That(question3.GetParent().PublicKey, Is.EqualTo(questionnaire.QuestionnaireDocument.CoverPageSectionId));
            Assert.That(question3.Featured, Is.True);
            Assert.That(cover.Children.Second(), Is.EqualTo(question3));
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