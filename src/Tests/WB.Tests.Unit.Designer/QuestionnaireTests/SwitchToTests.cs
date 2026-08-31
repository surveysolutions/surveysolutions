using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.SharedKernels.Questionnaire.Categories;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Tests.Abc;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.QuestionnaireTests;

[TestFixture]
internal class SwitchToTests : QuestionnaireTestsContext
{
    [Test]
    public void SwitchTo_when_questionnaire_dont_have_all_translation_texts_should_throw_exception()
    {
        // Arrange
        Guid responsibleId = Guid.NewGuid();
        Guid questionnaireId = Guid.NewGuid();
        Guid translationId = Guid.NewGuid();
        
        var questionnaireDocument = CreateQuestionnaireDocument(createdBy: responsibleId);
        questionnaireDocument.Translations = new List<Translation>()
        {
            Create.Translation(translationId: translationId, name: "Translation_Title")
        };
        
        var questionnaire = Create.Questionnaire();
        questionnaire.Initialize(questionnaireId, questionnaireDocument, []);
        var command = Create.Command.SwitchToTranslation(questionnaireId, responsibleId, translationId);
            
        // Act
        var exception = Assert.Catch<QuestionnaireException>(() => questionnaire.SwitchToTranslation(command));

        // Assert
        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.ErrorType, Is.EqualTo(DomainExceptionType.TranslationIsNotFull));
    }
    
    [Test]
    public void SwitchTo_when_switch_to_new_translation_should_translate_document()
    {
        // Arrange
        Guid responsibleId = Guid.NewGuid();
        Guid questionnaireId = Guid.NewGuid();
        Guid translationId = Id.g1;
        var questionnaireDocumentDefaultLanguageName = "TestName";
        
        var questionnaireDocument = CreateQuestionnaireDocument(createdBy: responsibleId);
        questionnaireDocument.DefaultLanguageName = questionnaireDocumentDefaultLanguageName;
        questionnaireDocument.Translations = new List<Translation>()
        {
            Create.Translation(translationId: translationId, name: "Translation_Title")
        };

        ITranslation translation = Mock.Of<ITranslation>();
        
        // var questionnaireTranslator = Mock.Of<IQuestionnaireTranslator>(t =>
        //     t.Translate(It.IsAny<QuestionnaireDocument>(), translation, false) == translatedDocument);
        var questionnaireTranslator = new Mock<IQuestionnaireTranslator>();
        questionnaireTranslator.Setup(x => x.Translate(It.IsAny<QuestionnaireDocument>(), translation, false))
            .Returns<QuestionnaireDocument, ITranslation, bool>((d, t, f) => d);
        
        var translationsService = Mock.Of<ITranslationsService>(s =>
            s.Get(questionnaireDocument.PublicKey, translationId) == translation);
        
        var designerTranslationService = Mock.Of<IDesignerTranslationService>(s =>
            s.IsFullTranslated(It.IsAny<QuestionnaireDocument>(), translation) == true);
        
        var questionnaire = Create.Questionnaire(questionnaireTranslator: questionnaireTranslator.Object, 
            translationsService: translationsService, designerTranslationService: designerTranslationService);
        questionnaire.Initialize(questionnaireId, questionnaireDocument, []);
        var command = Create.Command.SwitchToTranslation(questionnaireId, responsibleId, translationId);
            
        // Act
        questionnaire.SwitchToTranslation(command);

        // Assert
        Assert.That(questionnaire.QuestionnaireDocument, Is.Not.Null);
        Assert.That(questionnaire.QuestionnaireDocument.Translations.Count, Is.EqualTo(1));
        Assert.That(questionnaire.QuestionnaireDocument.Translations[0].Id, Is.Not.EqualTo(translationId));
        Assert.That(questionnaire.QuestionnaireDocument.Translations[0].Name, Is.EqualTo(questionnaireDocumentDefaultLanguageName));
        Assert.That(questionnaire.QuestionnaireDocument.DefaultLanguageName, Is.EqualTo("Translation_Title"));
    }

    [Test]
    public void SwitchTo_when_questionnaire_has_reusable_categories_should_copy_translations_to_new_categories_id()
    {
        var responsibleId = Guid.NewGuid();
        var questionnaireId = Guid.NewGuid();
        var translationId = Guid.NewGuid();
        var categoriesId = Guid.NewGuid();

        var questionnaireDocument = CreateQuestionnaireDocument(createdBy: responsibleId);
        questionnaireDocument.Categories = new List<Categories>
        {
            new() { Id = categoriesId, Name = "categories" }
        };
        questionnaireDocument.Translations = new List<Translation>
        {
            Create.Translation(translationId: translationId, name: "Translation")
        };

        var translation = new Mock<ITranslation>();
        translation.Setup(x => x.GetCategoriesText(categoriesId, 1, null)).Returns("translated");

        var translationsService = Mock.Of<ITranslationsService>(x =>
            x.Get(questionnaireDocument.PublicKey, translationId) == translation.Object);
        var designerTranslationService = new Mock<IDesignerTranslationService>();
        designerTranslationService.Setup(x => x.IsFullTranslated(It.IsAny<QuestionnaireDocument>(), translation.Object))
            .Returns(true);
        designerTranslationService.Setup(x => x.GetFromQuestionnaire(It.IsAny<QuestionnaireDocument>()))
            .Returns(Array.Empty<TranslationInstance>());

        var reusableCategoriesService = new Mock<IReusableCategoriesService>();
        reusableCategoriesService.Setup(x => x.GetCategoriesById(questionnaireDocument.PublicKey, categoriesId))
            .Returns(new[] { new CategoriesItem { Id = 1, Text = "original" } }.AsQueryable());

        var questionnaireTranslator = new Mock<IQuestionnaireTranslator>();
        questionnaireTranslator.Setup(x => x.Translate(It.IsAny<QuestionnaireDocument>(), translation.Object, false))
            .Returns<QuestionnaireDocument, ITranslation, bool>((document, _, _) => document);

        var questionnaire = Create.Questionnaire(
            questionnaireTranslator: questionnaireTranslator.Object,
            translationsService: translationsService,
            designerTranslationService: designerTranslationService.Object,
            reusableCategoriesService: reusableCategoriesService.Object);
        questionnaire.Initialize(questionnaireId, questionnaireDocument, []);

        questionnaire.SwitchToTranslation(Create.Command.SwitchToTranslation(
            questionnaireId, responsibleId, translationId));

        var newCategoriesId = questionnaire.QuestionnaireDocument.Categories.Single().Id;
        Assert.That(newCategoriesId, Is.Not.EqualTo(categoriesId));
        designerTranslationService.Verify(x => x.CopyCategoriesTranslations(
            questionnaireDocument.PublicKey, categoriesId, newCategoriesId), Times.Once);
    }
}
