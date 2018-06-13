using System;
using Main.Core.Documents;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.Infrastructure.PlainStorage;


namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.TranslationServiceTests
{
    internal class when_getting_translation_file_for_empty_questionnaire : TranslationsServiceTestsContext
    {
        [Test]
        public void should_not_throw_any_exceptions()
        {
            questionnaireId = Guid.Parse("11111111111111111111111111111111");

            var questionnaires = new Mock<IPlainKeyValueStorage<QuestionnaireDocument>>();
            questionnaires.SetReturnsDefault(Create.QuestionnaireDocument(questionnaireId));

            service = Create.TranslationsService(questionnaireStorage: questionnaires.Object);

            service.GetAsExcelFile(questionnaireId, Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD"));
        }

        static TranslationsService service;
        static Guid questionnaireId;
        static Exception exception;
    }
}
