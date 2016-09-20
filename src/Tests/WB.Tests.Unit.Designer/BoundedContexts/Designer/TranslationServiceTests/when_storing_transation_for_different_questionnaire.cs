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
    internal class when_storing_transation_for_different_questionnaire : TranslationsServiceTestsContext
    {
        Establish context = () =>
        {
            var questionnaire = Create.QuestionnaireDocument(Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC"), children: new IComposite[]
            {
                Create.Group(groupId: Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB"))
            });
            translationId = Guid.Parse("11111111111111111111111111111111");
            questionnaire.Translations.Add(Create.Translation(translationId));

            var testType = typeof(when_storing_translations_from_excel_file_with_missing_same_translations);
            var readResourceFile = $"{testType.Namespace}.translationsForDifferentQuestionnaire.xlsx";
            using (var manifestResourceStream = testType.Assembly.GetManifestResourceStream(readResourceFile))
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    manifestResourceStream.CopyTo(memoryStream);
                    fileStream = memoryStream.ToArray();
                }
            }

            plainStorageAccessor = new TestPlainStorage<TranslationInstance>();

            var questionnaires = new Mock<IPlainKeyValueStorage<QuestionnaireDocument>>();
            questionnaires.SetReturnsDefault(questionnaire);
            service = Create.TranslationsService(plainStorageAccessor,
                questionnaireStorage: questionnaires.Object);
        };

        Because of = () => service.Store(Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC"), translationId, fileStream);

        It should_not_store_entities_from_other_questionnare = () => plainStorageAccessor.Query(_ => _.Count()).ShouldEqual(0);

        static byte[] fileStream;
        static TestPlainStorage<TranslationInstance> plainStorageAccessor;
        static TranslationsService service;
        static Guid translationId;
    }
}