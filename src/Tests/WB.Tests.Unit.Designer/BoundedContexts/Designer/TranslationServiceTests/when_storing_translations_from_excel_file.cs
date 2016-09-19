using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.Questionnaire.Translations;
using It = Machine.Specifications.It;
using TranslationInstance = WB.Core.BoundedContexts.Designer.Translations.TranslationInstance;

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

            var questionnaire = Create.QuestionnaireDocument(Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC"), children: new IComposite[]
            {
                Create.Group(groupId: Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")),
                Create.SingleQuestion(id: questionId, variable: "question", isFilteredCombobox: true, options: new List<Answer> {Create.Option("1", "Combobox Option")})
            });

            var questionnaires = new Mock<IPlainKeyValueStorage<QuestionnaireDocument>>();
            questionnaires.SetReturnsDefault(questionnaire);

            service = Create.TranslationsService(plainStorageAccessor, questionnaires.Object);
            
        };

        Because of = () => service.Store(questionnaireId, translationId, fileStream);

        It should_store_all_entities_for_questionnaire_and_culture = () => 
            plainStorageAccessor.Query(_ => _.All(x => x.QuestionnaireId == questionnaireId && x.TranslationId == translationId)).ShouldBeTrue();

        It should_store_title_translation = () =>
        {
            var translationInstance = plainStorageAccessor.Query(_ => _.Single(x => x.Type == TranslationType.Title));
            translationInstance.QuestionnaireEntityId.ShouldEqual(entityId);
            translationInstance.Value.ShouldEqual("title");
            translationInstance.TranslationIndex.ShouldBeNull();
        };

        It should_store_instruction_translation = () =>
        {
            var translationInstance = plainStorageAccessor.Query(_ => _.Single(x => x.Type == TranslationType.Instruction));
            translationInstance.QuestionnaireEntityId.ShouldEqual(entityId);
            translationInstance.Value.ShouldEqual("instruction");
            translationInstance.TranslationIndex.ShouldBeNull();
        };
        
        It should_store_validation_translation = () =>
        {
            var translationInstance = plainStorageAccessor.Query(_ => _.Single(x => x.Type == TranslationType.ValidationMessage));
            translationInstance.QuestionnaireEntityId.ShouldEqual(entityId);
            translationInstance.Value.ShouldEqual("validation message");
            translationInstance.TranslationIndex.ShouldEqual("1");
        };


        It should_store_option_translation = () =>
        {
            var translationInstance = plainStorageAccessor.Query(_ => _.First(x => x.Type == TranslationType.OptionTitle));
            translationInstance.QuestionnaireEntityId.ShouldEqual(entityId);
            translationInstance.Value.ShouldEqual("option");
            translationInstance.TranslationIndex.ShouldEqual("2");
        };

        It should_store_option_translation_for_combobox_from_second_sheet = () =>
        {
            var translationInstance = plainStorageAccessor.Query(_ => _.Last(x => x.Type == TranslationType.OptionTitle));
            translationInstance.QuestionnaireEntityId.ShouldEqual(questionId);
            translationInstance.Value.ShouldEqual("Опция Комбобокса");
            translationInstance.TranslationIndex.ShouldEqual("1");
        };

        static TranslationsService service;
        static TestPlainStorage<TranslationInstance> plainStorageAccessor;
        static byte[] fileStream;
        static Guid questionnaireId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        static Guid entityId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        static Guid translationId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
    }
}