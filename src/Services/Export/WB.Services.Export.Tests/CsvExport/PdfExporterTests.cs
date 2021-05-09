using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Dropbox.Api.TeamLog;
using Moq;
using NUnit.Framework;
using WB.Services.Export.CsvExport.Exporters;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Services;
using WB.ServicesIntegration.Export;

namespace WB.Services.Export.Tests.CsvExport
{
    [TestOf(typeof(PdfExporter))]
    public class PdfExporterTests
    {
        [Test]
        public async Task should_export_pdf_file()
        {
            var questionnaire = Create.QuestionnaireDocument();
            questionnaire.VariableName = "qVariable";
            var translationId = Id.g1;
            questionnaire.Translations.Add(new Translation
            {
                Id = translationId,
                Name = "language name"
            });

            var hqApi = new Mock<IHeadquartersApi>();
            var mainPdfFileFromHq = new byte[]{1,2,3};
            var translatedPdfFileFromHq = new byte[]{4,5,6};
            hqApi.Setup(x => x.GetPdfAsync(questionnaire.QuestionnaireId, null))
                .ReturnsAsync(new MemoryStream(mainPdfFileFromHq));
            hqApi.Setup(x => x.GetPdfAsync(questionnaire.QuestionnaireId, translationId))
                .ReturnsAsync(new MemoryStream(translatedPdfFileFromHq));

            var fileSystem = new Mock<IFileSystemAccessor>();
            fileSystem.Setup(x => x.MakeValidFileName(It.IsAny<string>()))
                .Returns<string>(arg => arg);
            var mockMainStream = new MemoryStream();
            var mockTranslatedStream = new MemoryStream();
            fileSystem.Setup(x => x.OpenOrCreateFile($"testPath{Path.DirectorySeparatorChar}Questionnaire{Path.DirectorySeparatorChar}Pdf{Path.DirectorySeparatorChar}Original {questionnaire.VariableName}.pdf", false))
                .Returns(mockMainStream);
            fileSystem.Setup(x => x.OpenOrCreateFile($"testPath{Path.DirectorySeparatorChar}Questionnaire{Path.DirectorySeparatorChar}Pdf{Path.DirectorySeparatorChar}language name {questionnaire.VariableName}.pdf", false))
                .Returns(mockTranslatedStream);

            var service = Create.PdfExporter(Create.TenantHeadquartersApi(hqApi.Object), 
                fileSystem.Object);

            // Act
            await service.ExportAsync(Create.Tenant(), questionnaire, "testPath", CancellationToken.None);

            // Assert
            Assert.That(mockMainStream.ToArray(), Is.EquivalentTo(mainPdfFileFromHq), "Main pdf file should be written to export folder");
            Assert.That(mockTranslatedStream.ToArray(), Is.EquivalentTo(translatedPdfFileFromHq), "Translated pdf file should be written to export folder");
        }
    }
}
