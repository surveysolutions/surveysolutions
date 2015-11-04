using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;
using It = Moq.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.DataExport.ReadSideToTabularFormatExportServiceTests
{
    [Subject(typeof(ReadSideToTabularFormatExportService))]
    internal class ReadSideToTabularFormatExportServiceTestContext
    {
        protected static ReadSideToTabularFormatExportService CreateReadSideToTabularFormatExportService(
            IFileSystemAccessor fileSystemAccessor=null,
            ICsvWriterService csvWriterService = null,
            IQueryableReadSideRepositoryReader<InterviewStatuses> interviewStatuses=null,
             QuestionnaireExportStructure questionnaireExportStructure = null,
            IQueryableReadSideRepositoryReader<InterviewCommentaries> interviewCommentaries=null)
        {
            return new ReadSideToTabularFormatExportService(
                Mock.Of<ITransactionManagerProvider>(_ => _.GetTransactionManager() == Mock.Of<ITransactionManager>()),
                fileSystemAccessor ?? Mock.Of<IFileSystemAccessor>(),
                Mock.Of<ICsvWriterFactory>(_ => _.OpenCsvWriter(
                    It.IsAny<Stream>(), It.IsAny<string>()) == (csvWriterService ?? Mock.Of<ICsvWriterService>())),
                Mock.Of<ISerializer>(),new InterviewDataExportSettings("",false,10000, 100), 
                new TestInMemoryWriter<InterviewExportedDataRecord>(),
                interviewStatuses ?? Mock.Of<IQueryableReadSideRepositoryReader<InterviewStatuses>>(),
                interviewCommentaries??new TestInMemoryWriter<InterviewCommentaries>(),
                Mock.Of<IReadSideKeyValueStorage<QuestionnaireExportStructure>>(_ => _.GetById(
                    It.IsAny<string>()) == questionnaireExportStructure),
                new TestInMemoryWriter<InterviewSummary>(), 
                new TestInMemoryWriter<InterviewData>(),
                Mock.Of<IExportViewFactory>(),
                Mock.Of<IDataExportWriter>(),
                Mock.Of<ITransactionManager>(),
                new TestInMemoryWriter<QuestionnaireDocumentVersioned>()
                );
        }

        protected static HeaderStructureForLevel CreateHeaderStructureForLevel(string levelName = "table name", string[] referenceNames = null, ValueVector<Guid> levelScopeVector = null)
        {
            return new HeaderStructureForLevel()
            {
                LevelScopeVector = levelScopeVector ?? new ValueVector<Guid>(),
                LevelName = levelName,
                LevelIdColumnName = "Id",
                IsTextListScope = referenceNames != null,
                ReferencedNames = referenceNames,
                HeaderItems =
                    new Dictionary<Guid, ExportedHeaderItem>
                    {
                        { Guid.NewGuid(), CreateExportedHeaderItem() },
                        { Guid.NewGuid(), CreateExportedHeaderItem(QuestionType.Numeric, new[] { "a" }) }
                    }
            };
        }

        protected static ExportedHeaderItem CreateExportedHeaderItem(QuestionType type = QuestionType.Text, string[] columnNames = null)
        {
            return new ExportedHeaderItem() { ColumnNames = columnNames ?? new[] { "1" }, Titles = columnNames ?? new[] { "1" }, QuestionType = type };
        }

        protected static QuestionnaireExportStructure CreateQuestionnaireExportStructure(params HeaderStructureForLevel[] levels)
        {
            var header = new Dictionary<ValueVector<Guid>, HeaderStructureForLevel>();
            if (levels != null && levels.Length > 0)
            {
                header = levels.ToDictionary((i) => i.LevelScopeVector, (i) => i);
            }
            return new QuestionnaireExportStructure() { HeaderToLevelMap = header };
        }

        protected class CsvWriterServiceTestable : ICsvWriterService
        {
            private readonly List<object[]> recorderRows = new List<object[]>();
            private readonly List<object> currentRow = new List<object>();

            public void Dispose()
            {
            }

            public void WriteField<T>(T cellValue)
            {
                currentRow.Add(cellValue);
            }

            public void NextRecord()
            {
                recorderRows.Add(currentRow.ToArray());
                currentRow.Clear();
            }

            public List<object[]> Rows
            {
                get
                {
                    var result = recorderRows.ToList();
                    if (currentRow.Count > 0)
                        result.Add(currentRow.ToArray());
                    return result;
                }
            }
        }
    }
}