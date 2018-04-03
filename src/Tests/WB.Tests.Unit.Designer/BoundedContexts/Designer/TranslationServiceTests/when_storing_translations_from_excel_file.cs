using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.Questionnaire.Translations;

using TranslationInstance = WB.Core.BoundedContexts.Designer.Translations.TranslationInstance;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.TranslationServiceTests
{
    internal class when_storing_translations_from_excel_file : TranslationsServiceTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
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
            BecauseOf();

        }

        private void BecauseOf() => service.Store(questionnaireId, translationId, fileStream);

        [NUnit.Framework.Test] public void should_store_all_entities_for_questionnaire_and_culture () => 
            plainStorageAccessor.Query(_ => _.All(x => x.QuestionnaireId == questionnaireId && x.TranslationId == translationId)).Should().BeTrue();

        [NUnit.Framework.Test] public void should_store_title_translation () 
        {
            var translationInstance = plainStorageAccessor.Query(_ => _.Single(x => x.Type == TranslationType.Title));
            translationInstance.QuestionnaireEntityId.Should().Be(entityId);
            translationInstance.Value.Should().Be("title");
            translationInstance.TranslationIndex.Should().BeNull();
        }

        [NUnit.Framework.Test] public void should_store_instruction_translation () 
        {
            var translationInstance = plainStorageAccessor.Query(_ => _.Single(x => x.Type == TranslationType.Instruction));
            translationInstance.QuestionnaireEntityId.Should().Be(entityId);
            translationInstance.Value.Should().Be("instruction");
            translationInstance.TranslationIndex.Should().BeNull();
        }
        
        [NUnit.Framework.Test] public void should_store_validation_translation () 
        {
            var translationInstance = plainStorageAccessor.Query(_ => _.Single(x => x.Type == TranslationType.ValidationMessage));
            translationInstance.QuestionnaireEntityId.Should().Be(entityId);
            translationInstance.Value.Should().Be("validation message");
            translationInstance.TranslationIndex.Should().Be("1");
        }


        [NUnit.Framework.Test] public void should_store_option_translation () 
        {
            var translationInstance = plainStorageAccessor.Query(_ => _.First(x => x.Type == TranslationType.OptionTitle));
            translationInstance.QuestionnaireEntityId.Should().Be(entityId);
            translationInstance.Value.Should().Be("option");
            translationInstance.TranslationIndex.Should().Be("2");
        }

        [NUnit.Framework.Test] public void should_store_option_translation_for_combobox_from_second_sheet () 
        {
            var translationInstance = plainStorageAccessor.Query(_ => _.Last(x => x.Type == TranslationType.OptionTitle));
            translationInstance.QuestionnaireEntityId.Should().Be(questionId);
            translationInstance.Value.Should().Be("Опция Комбобокса");
            translationInstance.TranslationIndex.Should().Be("1");
        }

        static TranslationsService service;
        static TestPlainStorage<TranslationInstance> plainStorageAccessor;
        static byte[] fileStream;
        static Guid questionnaireId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        static Guid entityId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        static Guid translationId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
    }
}