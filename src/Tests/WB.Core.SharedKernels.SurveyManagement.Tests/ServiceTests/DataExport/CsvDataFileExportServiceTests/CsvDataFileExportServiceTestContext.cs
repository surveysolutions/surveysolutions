using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.ServiceTests.DataExport.CsvDataFileExportServiceTests
{
    internal class CsvDataFileExportServiceTestContext
    {
        protected static CsvDataFileExportService CreateCsvDataFileExportService(IFileSystemAccessor fileSystemAccessor=null, ICsvWriterService csvWriterService = null)
        {
            return new CsvDataFileExportService(fileSystemAccessor ?? Mock.Of<IFileSystemAccessor>(),
                Mock.Of<ICsvWriterFactory>(_ => _.OpenCsvWriter(It.IsAny<Stream>()) == csvWriterService));
        }

        protected static InterviewDataExportLevelView CreateInterviewDataExportLevelView(InterviewDataExportRecord[] records=null)
        {
            return new InterviewDataExportLevelView(new ValueVector<Guid>(), "level name", records?? new InterviewDataExportRecord[0]);
        }

        protected static InterviewDataExportRecord CreateInterviewDataExportRecord(string recordId=null,ExportedQuestion[] questions = null, string[] referenceValues = null, string[] parentLevelIds=null)
        {
            return new InterviewDataExportRecord(Guid.NewGuid(), recordId ?? "recordid", referenceValues ?? new string[0],
                parentLevelIds ?? new string[0],
                questions ?? new ExportedQuestion[0]);
        }

        protected static ExportedQuestion CreateExportedQuestion(string[] answers = null)
        {
            return new ExportedQuestion(Guid.NewGuid(), answers ?? new string[0]);
        }

        protected static InterviewActionExportView CreateInterviewActionExportView(string interviewId,
            InterviewExportedAction action = InterviewExportedAction.InterviewerAssigned, string originator = null,
            DateTime? timeStamp = null, string role = null)
        {
            return new InterviewActionExportView(interviewId, action, originator ?? "originator", timeStamp ?? DateTime.Now, role);
        }

        protected static HeaderStructureForLevel CreateHeaderStructureForLevel(string levelIdColumnName = null, string[] referencedNames = null, IDictionary<Guid, ExportedHeaderItem> headerItems = null, ValueVector<Guid> levelScopeVector = null)
        {
            return new HeaderStructureForLevel()
            {
                LevelIdColumnName = levelIdColumnName,
                IsTextListScope = true,
                ReferencedNames = referencedNames,
                HeaderItems = headerItems,
                LevelScopeVector = levelScopeVector
            };
        }

        protected static ExportedHeaderItem CreateExportedHeaderItem(string[] columnNames=null)
        {
            return new ExportedHeaderItem() { ColumnNames = columnNames };
        }
    }
}
