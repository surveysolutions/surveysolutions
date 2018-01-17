using System;
using System.IO;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.TranslationServiceTests
{
    [TestOf(typeof(TranslationsService))]
    internal class TranslationsServiceTests : TranslationsServiceTestsContext
    {
        [Test]
        public void when_storing_translations_with_html_from_excel_file()
        {
            //assert
            Guid questionnaireId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            Guid translationId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");

            var testType = typeof(when_storing_translations_from_excel_file);
            var readResourceFile = testType.Namespace + ".testTranslationsWithHtml.xlsx";
            var manifestResourceStream = testType.Assembly.GetManifestResourceStream(readResourceFile);

            byte[] fileStream;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                manifestResourceStream.CopyTo(memoryStream);
                fileStream = memoryStream.ToArray();
            }

            var plainStorageAccessor = new TestPlainStorage<TranslationInstance>();

            var questionnaire = Create.QuestionnaireDocument(Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC"), children: new IComposite[]
            {
                Create.Group(groupId: Guid.Parse("11111111111111111111111111111111"), title:"Section Text"),
            });

            var questionnaires = new Mock<IPlainKeyValueStorage<QuestionnaireDocument>>();
            questionnaires.SetReturnsDefault(questionnaire);

            var service = Create.TranslationsService(plainStorageAccessor, questionnaires.Object);

            //act
            service.Store(questionnaireId, translationId, fileStream);

            //assert
            Assert.That(plainStorageAccessor.Query(_ => _.FirstOrDefault()).Value, Is.EqualTo("Текст секции"));
        }
    }
}