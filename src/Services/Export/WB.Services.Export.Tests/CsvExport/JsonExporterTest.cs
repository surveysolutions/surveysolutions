using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WB.Services.Export.CsvExport.Exporters;
using WB.Services.Export.Infrastructure;

namespace WB.Services.Export.Tests.CsvExport
{
    [TestOf(typeof(JsonExporter))]
    public class JsonExporterTest
    {
        [Test]
        public async Task should_export_Json()
        {
            var questionnaire = Create.QuestionnaireDocument();
            questionnaire.VariableName = "qVariable";
            
            var fileSystem = new Mock<IFileSystemAccessor>();
            fileSystem.Setup(x => x.MakeValidFileName(It.IsAny<string>()))
                .Returns<string>(arg => arg);

            var service = Create.JsonExporter(fileSystem.Object);

            // Act
            await service.ExportAsync(questionnaire, "testPath", CancellationToken.None);

            fileSystem.Verify(v => v.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>()),
                Times.Once, 
                "Should be called");
        }
    }
}
