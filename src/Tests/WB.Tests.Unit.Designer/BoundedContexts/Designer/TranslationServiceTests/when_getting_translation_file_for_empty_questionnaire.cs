using System;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.TranslationServiceTests
{
    internal class when_getting_translation_file_for_empty_questionnaire : TranslationsServiceTestsContext
    {
        Establish context = () =>
        {
            questionnaireId = Guid.Parse("11111111111111111111111111111111");
            
            var questionnaires = new Mock<IPlainKeyValueStorage<QuestionnaireDocument>>();
            questionnaires.SetReturnsDefault(Create.QuestionnaireDocument(questionnaireId));

            service = Create.TranslationsService(questionnaireStorage: questionnaires.Object);
        };

        Because of = () => exception = Catch.Exception(()=> service.GetAsExcelFile(questionnaireId, Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD")));

        It should_not_throw_any_exceptions = () => exception.ShouldBeNull();
        
        static TranslationsService service;
        static Guid questionnaireId;
        static Exception exception;
    }
}