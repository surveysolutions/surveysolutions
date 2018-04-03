using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Moq;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.Questionnaire.Translations;

using TranslationInstance = WB.Core.BoundedContexts.Designer.Translations.TranslationInstance;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.TranslationServiceTests
{
    internal class when_storing_translations_from_excel_file_with_missing_same_translations : TranslationsServiceTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var testType = typeof(when_storing_translations_from_excel_file_with_missing_same_translations);
            var readResourceFile = testType.Namespace + ".testTranslationsWithMissingTranslations.xlsx";
            var manifestResourceStream = testType.Assembly.GetManifestResourceStream(readResourceFile);
            using (MemoryStream memoryStream = new MemoryStream())
            {
                manifestResourceStream.CopyTo(memoryStream);
                fileStream = memoryStream.ToArray();
            }

            plainStorageAccessor = new TestPlainStorage<TranslationInstance>();
            var questionnaire = Create.QuestionnaireDocument(Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC"), children: new IComposite[]
            {
                Create.Group(groupId: Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"))
            });

            var questionnaires = new Mock<IPlainKeyValueStorage<QuestionnaireDocument>>();
            questionnaires.SetReturnsDefault(questionnaire);

            service = Create.TranslationsService(plainStorageAccessor, questionnaires.Object);
            BecauseOf();

        }

        private void BecauseOf() => service.Store(questionnaireId, translationId, fileStream);

        [NUnit.Framework.Test] public void should_store_all_entities_for_questionnaire_and_culture () => 
            plainStorageAccessor.Query(_ => _.All(x => x.QuestionnaireId == questionnaireId && x.TranslationId == translationId)).Should().BeTrue();

        [NUnit.Framework.Test] public void should_dont_store_title_translation () 
        {
            var translationInstance = plainStorageAccessor.Query(_ => _.SingleOrDefault(x => x.Type == TranslationType.Title));
            translationInstance.Should().BeNull();
        }

        [NUnit.Framework.Test] public void should_dont_store_instruction_translation () 
        {
            var translationInstance = plainStorageAccessor.Query(_ => _.SingleOrDefault(x => x.Type == TranslationType.Instruction));
            translationInstance.Should().BeNull();
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
            var translationInstance = plainStorageAccessor.Query(_ => _.Single(x => x.Type == TranslationType.OptionTitle));
            translationInstance.QuestionnaireEntityId.Should().Be(entityId);
            translationInstance.Value.Should().Be("option");
            translationInstance.TranslationIndex.Should().Be("2");
        }

        static TranslationsService service;
        static TestPlainStorage<TranslationInstance> plainStorageAccessor;
        static byte[] fileStream;
        static Guid questionnaireId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        static Guid entityId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        static Guid translationId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
    }
}