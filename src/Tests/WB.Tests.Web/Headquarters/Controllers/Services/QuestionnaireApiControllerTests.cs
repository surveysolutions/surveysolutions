using System.Collections.Generic;
using Main.Core.Documents;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Questionnaire.Categories;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Infrastructure.Native.Questionnaire;
using WB.Tests.Abc;
using WB.UI.Headquarters.Controllers.Services.Export;

namespace WB.Tests.Web.Headquarters.Controllers.Services
{
    [TestFixture]
    [TestOf(typeof(QuestionnaireApiController))]
    public class QuestionnaireApiControllerTests
    {
        [Test]
        public void when_questionnaire_with_translation_Should_return_translated_questionnaire()
        {
            ITranslation translation = Mock.Of<ITranslation>();
            
            var translatedQuestionnaire = Abc.Create.Entity.QuestionnaireDocument();
            var translator =
                Mock.Of<IQuestionnaireTranslator>(x =>
                    x.Translate(It.IsAny<QuestionnaireDocument>(), translation, false) == translatedQuestionnaire);

            var jsonForTranslatedQuestionnaire = "{translated: true}";
            var serializer = Mock.Of<ISerializer>(x => x.Serialize(translatedQuestionnaire) == jsonForTranslatedQuestionnaire);

            var questionnaireIdentity = Abc.Create.Entity.QuestionnaireIdentity();
            var translationId = Id.g1;

            var translationStorage = Mock.Of<ITranslationStorage>(x => x.Get(questionnaireIdentity, translationId) == translation);
            var questionnaireApiController = Create.Controller.QuestionnaireApiController(serializer: serializer,
                translator: translator,
                translationStorage: translationStorage);
            
            // Act
            var actionResult = questionnaireApiController.Get(questionnaireIdentity.ToString(), translationId) as ContentResult;

            // Assert
            Assert.That(actionResult.ContentType, Is.EqualTo("application/json"));
            Assert.That(actionResult.Content, Is.EqualTo(jsonForTranslatedQuestionnaire));
        }

        [Test]
        public void when_getting_reusable_category_with_translation_Should_apply_translation_on_option_text()
        {
            var questionnaireIdentity = Abc.Create.Entity.QuestionnaireIdentity();
            var categoryId = Id.g1;
            var translationId = Id.g2;

            var categoriesStorage = Mock.Of<IReusableCategoriesStorage>(x =>
                x.GetOptions(questionnaireIdentity, categoryId) == new List<CategoriesItem>
                {
                    Abc.Create.Entity.CategoriesItem("item1", 1, null),
                    Abc.Create.Entity.CategoriesItem("item2", 2, null)
                });

            var translationForCategoryText = "переведен";
            ITranslation translation = Mock.Of<ITranslation>(x => x.GetCategoriesText(categoryId, 1, null) == translationForCategoryText);
            var translationStorage = Mock.Of<ITranslationStorage>(x => x.Get(questionnaireIdentity, translationId) == translation);
            
            var questionnaireApiController = Create.Controller.QuestionnaireApiController(
                reusableCategoriesStorage: categoriesStorage,
                translationStorage: translationStorage);
            
            // Act
            var actionResult = questionnaireApiController.Category(questionnaireIdentity.ToString(), categoryId,
                translationId);

            // Assert
            Assert.That(actionResult.Value, Is.Not.Null.Or.Empty);
            Assert.That(actionResult.Value, Has.Count.EqualTo(2));
            Assert.That(actionResult.Value[0].Text, Is.EqualTo(translationForCategoryText));
            Assert.That(actionResult.Value[1].Text, Is.EqualTo("item2"));
        }
    }
}
