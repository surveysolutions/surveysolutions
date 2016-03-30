using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using Machine.Specifications;
using NHibernate;
using NSubstitute;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services.Exporters;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.Export;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.InterviewsExporterTests
{
    [Subject(typeof(InterviewsExporter))]
    public class when_answer_contains_separator_in_content
    {
        Establish context = () =>
        {
            questionnaireExportStructure = Create.QuestionnaireExportStructure();
            var headerStructureForLevel = Create.HeaderStructureForLevel();
            headerStructureForLevel.LevelName = "";
            questionnaireExportStructure.HeaderToLevelMap.Add(new ValueVector<Guid>(), headerStructureForLevel);

            var item = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            interviewIdsToExport = new List<Guid> {item};

            var rowReader = Substitute.For<InterviewExportredDataRowReader>();

            var interviewDataExportRecord = Create.InterviewDataExportRecord(item);
            interviewDataExportRecord.Answers = new string[] {"1"+ ExportFileSettings.SeparatorOfExportedDataFile + "2"};

            rowReader.ReadExportDataForInterview(item)
                     .Returns(new List<InterviewDataExportRecord>
                     {
                         interviewDataExportRecord 
                     });

            csvWriterMock = Substitute.For<ICsvWriter>();
            exporter = new InterviewsExporter(Substitute.For<IFileSystemAccessor>(),
                Substitute.For<ILogger>(),
                new InterviewDataExportSettings(), 
                csvWriterMock,
                rowReader);
        };

        Because of = () => exporter.Export(questionnaireExportStructure, interviewIdsToExport, String.Empty, new Progress<int>(), CancellationToken.None);

        It should_handle_tabs_gracefuly = () => 
            csvWriterMock.Received().WriteData(Arg.Is(""), Arg.Is<IEnumerable<string[]>>(arg => arg.First().Second() == "12"), ExportFileSettings.SeparatorOfExportedDataFile.ToString());

        static InterviewsExporter exporter;
        static QuestionnaireExportStructure questionnaireExportStructure;
        static List<Guid> interviewIdsToExport;
        static ICsvWriter csvWriterMock;
    }
}