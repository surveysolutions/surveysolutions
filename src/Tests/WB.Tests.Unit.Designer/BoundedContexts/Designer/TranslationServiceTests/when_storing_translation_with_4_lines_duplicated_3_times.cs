using System;
using System.IO;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Moq;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.TranslationServiceTests
{
    internal class when_storing_translation_with_4_lines_duplicated_3_times : TranslationsServiceTestsContext
    {
        Establish context = () =>
        {
            var testType = typeof(when_storing_translations_from_excel_file);
            var readResourceFile = testType.Namespace + ".testTranslationsWithDuplicates.xlsx";
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

        };

        Because of = () => service.Store(questionnaireId, translationId, fileStream);

        It should_store_4_entities = () =>
            plainStorageAccessor.Query(_ => _.Count()).ShouldEqual(4);

        private static TranslationsService service;
        private static TestPlainStorage<TranslationInstance> plainStorageAccessor;
        private static byte[] fileStream;
        private static Guid questionnaireId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static Guid entityId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid translationId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
    }
}