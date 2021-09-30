using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Moq;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.Infrastructure.PlainStorage;


namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.TranslationServiceTests
{
    internal class when_storing_translation_with_4_lines_duplicated_3_times : TranslationsServiceTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var testType = typeof(when_storing_translations_from_excel_file);
            var readResourceFile = testType.Namespace + ".testTranslationsWithDuplicates.xlsx";
            var manifestResourceStream = testType.Assembly.GetManifestResourceStream(readResourceFile);
            using (MemoryStream memoryStream = new MemoryStream())
            {
                manifestResourceStream.CopyTo(memoryStream);
                fileStream = memoryStream.ToArray();
            }

            plainStorageAccessor = Create.InMemoryDbContext(); 

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

        [NUnit.Framework.Test] public void should_store_4_entities () =>
            plainStorageAccessor.TranslationInstances.Count().Should().Be(4);

        TranslationsService service;
        DesignerDbContext plainStorageAccessor;
        byte[] fileStream;
        Guid questionnaireId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        Guid entityId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        Guid translationId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
    }
}
