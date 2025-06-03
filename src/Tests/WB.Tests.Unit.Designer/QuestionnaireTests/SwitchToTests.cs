using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Translations;
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
}
