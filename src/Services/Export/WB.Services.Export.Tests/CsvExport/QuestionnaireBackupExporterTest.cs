using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WB.Services.Export.CsvExport.Exporters;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Services;
using WB.ServicesIntegration.Export;

namespace WB.Services.Export.Tests.CsvExport
{
    [TestOf(typeof(QuestionnaireBackupExporter))]
    public class QuestionnaireBackupExporterTest
    {
        [Test]
        public async Task should_export_QuestionnaireBackup()
        {
            var questionnaire = Create.QuestionnaireDocument();
            questionnaire.VariableName = "qVariable";


            var hqApi = new Mock<IHeadquartersApi>();
            var backupFileFromHq = new byte[] { 1, 2, 3 };
            
            hqApi.Setup(x => x.GetBackupAsync(questionnaire.QuestionnaireId))
                .ReturnsAsync(new MemoryStream(backupFileFromHq));
           
            var fileSystem = new Mock<IFileSystemAccessor>();
            fileSystem.Setup(x => x.MakeValidFileName(It.IsAny<string>()))
                .Returns<string>(arg => arg);

            var mockBackupStream = new MemoryStream();
            fileSystem.Setup(x => x.OpenOrCreateFile(It.IsAny<string>(), false))
                .Returns(mockBackupStream);


            var service = Create.QuestionnaireBackupExporter(fileSystem.Object, Create.TenantHeadquartersApi(hqApi.Object));

            // Act
            await service.ExportAsync(Create.Tenant(), questionnaire, "testPath", CancellationToken.None);

            fileSystem.Verify(v => v.OpenOrCreateFile(It.IsAny<string>(), false),
                Times.Once, 
                "Should be called");

            Assert.That(mockBackupStream.ToArray(), Is.EquivalentTo(backupFileFromHq), "Backup file should be written to export folder");
        }
    }
}
