using System;
using System.Linq;
using NUnit.Framework;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.QuestionnaireTests
{
    internal class CloneTests : QuestionnaireTestsContext
    {
        [Test]
        public void when_cloning_questionnaire_with_categories()
        {
            // arrange
            Guid responsibleId = Guid.Parse("DDDD0000000000000000000000000000");
            Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
            Guid clonedQuestionnaireId = Guid.Parse("22222222222222222222222222222222");
            Guid categoriesId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            string name = "name";

            var questionnaire = CreateQuestionnaire(questionnaireId: questionnaireId, responsibleId: responsibleId);

            var sourceQuestionnaire = Create.QuestionnaireDocument();
            sourceQuestionnaire.Categories.Add(Create.Categories(categoriesId, name));

            // act
            questionnaire.CloneQuestionnaire("Title", false, responsibleId, clonedQuestionnaireId, sourceQuestionnaire);

            // assert
            Assert.That(questionnaire.QuestionnaireDocument.Categories, Has.One.Items);
            Assert.That(questionnaire.QuestionnaireDocument.Categories.First().Id, Is.EqualTo(categoriesId));
            Assert.That(questionnaire.QuestionnaireDocument.Categories.First().Name, Is.EqualTo(name));
        }

        [Test]
        public void when_cloning_questionnaire_with_categories_and_question_depentent_to_that_categories()
        {
            // arrange
            Guid responsibleId = Guid.Parse("DDDD0000000000000000000000000000");
            Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
            Guid clonedQuestionnaireId = Guid.Parse("22222222222222222222222222222222");
            Guid categoriesId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            Guid categoricalSingleQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            var questionnaire = CreateQuestionnaire(questionnaireId: questionnaireId, responsibleId: responsibleId);

            var sourceQuestionnaire = Create.QuestionnaireDocumentWithOneChapter(Create.SingleOptionQuestion(categoricalSingleQuestionId, categoriesId: categoriesId));
            sourceQuestionnaire.Categories.Add(Create.Categories(categoriesId));

            // act
            questionnaire.CloneQuestionnaire("Title", false, responsibleId, clonedQuestionnaireId, sourceQuestionnaire);

            // assert
            Assert.That(questionnaire.QuestionnaireDocument.Categories.Find(x => x.Id == categoriesId), Is.Not.Null);
            Assert.That(questionnaire.QuestionnaireDocument.Find<ICategoricalQuestion>(categoricalSingleQuestionId)
                .CategoriesId, Is.EqualTo(categoriesId));
        }
    }
}
