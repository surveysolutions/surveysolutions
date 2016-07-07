using System;
using System.IO;
using System.Linq;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Translations;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.TranslationServiceTests
{
    internal class when_storing_translations_from_excel_file : TranslationsServiceTestsContext
    {
        Establish context = () =>
        {
            var testType = typeof(when_storing_translations_from_excel_file);
            var readResourceFile = testType.Namespace + ".testTranslations.xlsx";
            var manifestResourceStream = testType.Assembly.GetManifestResourceStream(readResourceFile);
            using (MemoryStream memoryStream = new MemoryStream())
            {
                manifestResourceStream.CopyTo(memoryStream);
                fileStream = memoryStream.ToArray();
            }

            plainStorageAccessor = new TestPlainStorage<TranslationInstance>();
            service = CreateTranslationsService(plainStorageAccessor);
            
        };

        Because of = () => service.Store(questionnaireId, "en-US", fileStream);

        It should_store_all_entities_for_questionnaire_and_culture = () => 
            plainStorageAccessor.Query(_ => _.All(x => x.QuestionnaireId == questionnaireId && x.Culture == "en-US")).ShouldBeTrue();

        It should_store_title_translation = () =>
        {
            var translationInstance = plainStorageAccessor.Query(_ => _.Single(x => x.Type == TranslationType.Title));
            translationInstance.QuestionnaireEntityId.ShouldEqual(entityId);
            translationInstance.Translation.ShouldEqual("title");
            translationInstance.TranslationIndex.ShouldBeNull();
        };

        It should_store_instruction_translation = () =>
        {
            var translationInstance = plainStorageAccessor.Query(_ => _.Single(x => x.Type == TranslationType.Instruction));
            translationInstance.QuestionnaireEntityId.ShouldEqual(entityId);
            translationInstance.Translation.ShouldEqual("instruction");
            translationInstance.TranslationIndex.ShouldBeNull();
        };

        It should_store_validation_translation = () =>
        {
            var translationInstance = plainStorageAccessor.Query(_ => _.Single(x => x.Type == TranslationType.ValidationMessage));
            translationInstance.QuestionnaireEntityId.ShouldEqual(entityId);
            translationInstance.Translation.ShouldEqual("validation message");
            translationInstance.TranslationIndex.ShouldEqual("1");
        };


        It should_store_option_translation = () =>
        {
            var translationInstance = plainStorageAccessor.Query(_ => _.Single(x => x.Type == TranslationType.OptionTitle));
            translationInstance.QuestionnaireEntityId.ShouldEqual(entityId);
            translationInstance.Translation.ShouldEqual("option");
            translationInstance.TranslationIndex.ShouldEqual("2");
        };

        static TranslationsService service;
        static TestPlainStorage<TranslationInstance> plainStorageAccessor;
        static byte[] fileStream;
        static Guid questionnaireId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        static Guid entityId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
    }
}