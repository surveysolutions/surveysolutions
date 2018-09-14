using System;
using System.Collections.Generic;
using System.Threading;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services.Exporters;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;
using WB.Tests.Abc.TestFactories;

namespace WB.Tests.Unit.DataExportTests.InterviewsExporterTests
{
    [TestFixture]
    [TestOf(typeof(InterviewsExporter))]
    internal class when_exporting_data_for_one_interview
    {
                [SetUp]
        public void SetUp()
        {
            dataInCsvFile = new List<CsvData>();

            fileSystemAccessor = new Mock<IFileSystemAccessor>();
            logger = new Mock<ILogger>();
            csvWriter = Create.Service.CsvWriter(dataInCsvFile);
            transactionManagerProvider = new Mock<ITransactionManagerProvider>();
            transactionManagerProvider.Setup(x => x.GetTransactionManager()).Returns(Mock.Of<ITransactionManager>());

            errorsExporter = new Mock<IInterviewErrorsExporter>();
            errorsExporter.Setup(x => x.Export(It.IsAny<QuestionnaireExportStructure>(), It.IsAny<List<InterviewEntity>>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(() => new List<string[]>());

            fileSystemAccessor
                .Setup(x => x.CombinePath(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((string b, string p) => p);
        }

        [Test]
        public void It_should_export_service_column_with_interview_key()
        {
            //arrange
            Guid interviewId = Id.g1;
            var interviewKey = "11-11-11-11";

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.NumericIntegerQuestion(variable: "q1")
            );

            var plQuestionnaire = Create.Entity.PlainQuestionnaire(questionnaire);

            var questionnaireExportStructure = Create.Entity.QuestionnaireExportStructure(questionnaire);
            var interviewIdsToExport = new List<InterviewToExport> { new InterviewToExport(interviewId, interviewKey, 1, InterviewStatus.Completed) };

            var exportViewFactory = new Mock<IExportViewFactory>();
            string[][] answers = { new string[1] };
            answers[0][0] = "1";

            exportViewFactory.SetReturnsDefault(Create.Entity.InterviewDataExportView(
                levels: Create.Entity.InterviewDataExportLevelView(interviewId, 
                    Create.Entity.InterviewDataExportRecord(interviewId, systemVariableValues: new []{"5"}, answers: answers))));

            var exporter = new InterviewsExporter(logger.Object,
                interviewDataExportSettings,
                csvWriter,
                transactionManagerProvider.Object,
                errorsExporter.Object,
                Mock.Of<IInterviewFactory>(),
                exportViewFactory.Object,
                Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(plQuestionnaire.QuestionnaireId, plQuestionnaire));

            //act
            exporter.Export(questionnaireExportStructure, interviewIdsToExport, "", new Progress<int>(), CancellationToken.None);

            //assert
            Assert.That(dataInCsvFile[0].File, Is.EqualTo("MyQuestionnaire.tab"));

            Assert.That(dataInCsvFile[0].Data[0][3], Is.EqualTo(ServiceColumns.Key));
            Assert.That(dataInCsvFile[1].Data[0][3], Is.EqualTo(interviewKey));

            Assert.That(dataInCsvFile[0].Data[0][4], Is.EqualTo(ServiceColumns.HasAnyError));
            Assert.That(dataInCsvFile[1].Data[0][4], Is.EqualTo("1"));

            Assert.That(dataInCsvFile[0].Data[0][5], Is.EqualTo(ServiceColumns.InterviewStatus));
            Assert.That(dataInCsvFile[1].Data[0][5], Is.EqualTo(InterviewStatus.Completed.ToString()));
        }

        private List<CsvData> dataInCsvFile;
        private Mock<IFileSystemAccessor> fileSystemAccessor;
        private Mock<ILogger> logger;
        private readonly InterviewDataExportSettings interviewDataExportSettings = new InterviewDataExportSettings("folder", false, 1, 1, 1, 1 ,10);
        private ICsvWriter csvWriter;
        private Mock<ITransactionManagerProvider> transactionManagerProvider;
        private Mock<IInterviewErrorsExporter> errorsExporter;
    }
}
