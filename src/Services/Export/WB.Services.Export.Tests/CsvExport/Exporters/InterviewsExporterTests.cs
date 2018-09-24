using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WB.Services.Export.CsvExport.Exporters;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Services;
using WB.Services.Export.Utils;
using WB.Tests.Abc;

namespace WB.Services.Export.Tests.CsvExport.Exporters
{
    [TestFixture]
    [TestOf(typeof(InterviewsExporter))]
    internal class InterviewsExporterTests
    {
        [SetUp]
        public void SetUp()
        {
            dataInCsvFile = new List<Create.CsvData>();

            csvWriter = Create.CsvWriter(dataInCsvFile);

            errorsExporter = new Mock<IInterviewErrorsExporter>();
            errorsExporter.SetupIgnoreArgs(x => x.Export(null, null, null, null, null))
                .Returns(() => new List<string[]>());
        }

        [Test]
        public async Task It_should_export_service_column_with_interview_key()
        {
            //arrange
            Guid interviewId = Id.g1;
            var interviewKey = "11-11-11-11";

            var questionnaire = Create.QuestionnaireDocument(
                variableName: "MyQuestionnaire"
            );

            var questionnaireExportStructure = Create.QuestionnaireExportStructure(questionnaire);
            var interviewIdsToExport = new List<InterviewToExport>
            {
                new InterviewToExport(interviewId, interviewKey, 1, InterviewStatus.Completed)
            };

            string[][] answers = { new string[1] };
            answers[0][0] = "1";

            var systemVariables = ServiceColumns.SystemVariables.Values;
            var systemVariableValues = new string[systemVariables.Count];
            systemVariableValues[ServiceColumns.SystemVariables[ServiceVariableType.InterviewRandom].Index] = "5";

            var interviewFactory = new Mock<IInterviewFactory>();
            interviewFactory.SetupIgnoreArgs(x => x.GetInterviewDataLevels(null, null))
                .Returns(new Dictionary<string, InterviewLevel>());

            var exporter = Create.InterviewsExporter(csvWriter, interviewFactory.Object);

            //act
            await exporter.ExportAsync(Create.Tenant(), questionnaireExportStructure, questionnaire, interviewIdsToExport, "", new Progress<int>(), CancellationToken.None);

            //assert
            Assert.That(dataInCsvFile[0].File, Is.EqualTo("MyQuestionnaire.tab"));

            Assert.That(dataInCsvFile[0].Data[0], Has.Length.EqualTo(dataInCsvFile[1].Data[0].Length),
                "Length of header columns should be equal to data columns length");

            Assert.That(dataInCsvFile[0].Data[0][2], Is.EqualTo(ServiceColumns.Key));
            Assert.That(dataInCsvFile[1].Data[0][2], Is.EqualTo(interviewKey));

            Assert.That(dataInCsvFile[0].Data[0][3], Is.EqualTo(ServiceColumns.HasAnyError));
            Assert.That(dataInCsvFile[1].Data[0][3], Is.EqualTo("1"));

            Assert.That(dataInCsvFile[0].Data[0][4], Is.EqualTo(ServiceColumns.InterviewStatus));
            Assert.That(dataInCsvFile[1].Data[0][4], Is.EqualTo(InterviewStatus.Completed.ToString()));
        }

        private List<Create.CsvData> dataInCsvFile;
        private ICsvWriter csvWriter;
        private Mock<IInterviewErrorsExporter> errorsExporter;
    }
}
