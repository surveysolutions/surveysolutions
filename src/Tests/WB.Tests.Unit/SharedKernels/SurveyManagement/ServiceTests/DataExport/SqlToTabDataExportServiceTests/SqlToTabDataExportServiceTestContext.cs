using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Services.Sql;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using It = Moq.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.DataExport.SqlToTabDataExportServiceTests
{
    [Subject(typeof (SqlToTabDataExportService))]
    internal class SqlToTabDataExportServiceTestContext
    {
        protected static SqlToTabDataExportService CreateSqlToTabDataExportService(ICsvWriterService csvWriterService = null, 
            QuestionnaireExportStructure questionnaireExportStructure = null,
            ISqlDataAccessor sqlDataAccessor = null,
            IFileSystemAccessor fileSystemAccessor = null,
            ITabFileReader tabFileReader = null,
            IDatasetWriterFactory datasetWriterFactory = null)
        {
            return new SqlToTabDataExportService(Mock.Of<IFileSystemAccessor>(),
                Mock.Of<ICsvWriterFactory>(_ => _.OpenCsvWriter(
                    It.IsAny<Stream>(), It.IsAny<string>()) == csvWriterService),
                Mock.Of<IExportedDataAccessor>(),
                Mock.Of<IReadSideKeyValueStorage<QuestionnaireExportStructure>>(_ => _.GetById(
                    It.IsAny<string>()) == questionnaireExportStructure),
                Mock.Of<IQueryableReadSideRepositoryReader<InterviewExportedDataRecord>>(),
                Mock.Of<IQueryableReadSideRepositoryReader<InterviewHistory>>(), Mock.Of<IJsonUtils>(),
                Mock.Of<ITransactionManagerProvider>(),
                Mock.Of<ILogger>(),
                tabFileReader ?? Mock.Of<ITabFileReader>(),
                datasetWriterFactory ?? Mock.Of<IDatasetWriterFactory>());
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
                    if(currentRow.Count>0)
                        result.Add(currentRow.ToArray());
                    return result;
                }
            }
        }
    }
}
