using System.IO;
using System.Linq;
using ClosedXML.Excel;
using Main.Core.Documents;
using Moq;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Tests.Abc;


namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.TranslationServiceTests
{
    internal class when_getting_translation_file_for_empty_questionnaire : TranslationsServiceTestsContext
    {
        [Test]
        public void should_not_throw_any_exceptions()
        {
            var questionnaireId = Id.g1;

            var questionnaires = new Mock<IPlainKeyValueStorage<QuestionnaireDocument>>();
            questionnaires.SetReturnsDefault(Create.QuestionnaireDocument(questionnaireId, title: "To be translated"));

            var service = Create.TranslationsService(questionnaireStorage: questionnaires.Object);

            TranslationFile excelFile = service.GetAsExcelFile(questionnaireId, Id.gD);
            
            IXLWorksheet workbook = new XLWorkbook(new MemoryStream(excelFile.ContentAsExcelFile)).Worksheets.First();
            
            var questionnaireTitleRow = 2;
            Assert.That(workbook.Cell(questionnaireTitleRow, translationTypeColumn).Value, 
                Is.EqualTo(TranslationType.Title.ToString()));
            Assert.That(workbook.Cell(questionnaireTitleRow, translationIndexColumn).Value, Is.Empty);
            Assert.That(workbook.Cell(questionnaireTitleRow, questionnaireEntityIdColumn).GetString(), Is.EqualTo(Id.g1.FormatGuid()));
            Assert.That(workbook.Cell(questionnaireTitleRow, originalTextColumn).GetString(), Is.EqualTo("To be translated"));
            Assert.That(workbook.Cell(questionnaireTitleRow, translactionColumn).Value, Is.Empty);
        }
    }
}
