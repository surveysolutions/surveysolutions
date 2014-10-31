using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.Files.Implementation.FileSystem;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Sql;
using WB.Core.SharedKernels.SurveyManagement.Services.Sql;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using It = Moq.It;

namespace WB.Tests.Integration.SqlDataExportWriterTests
{
    [Subject(typeof(SqlDataExportWriter))]
    internal class SqlDataExportWriterTestContext
    {
        protected static SqlDataExportWriter CreateSqlDataExportWriter(ISqlServiceFactory sqlServiceFactory)
        {
            var fileSystemAccessor = new FileSystemIOAccessor();
            return new SqlDataExportWriter(new SqlDataAccessor(fileSystemAccessor), sqlServiceFactory, fileSystemAccessor);
        }

        protected static SqliteServiceFactory CreateSqliteServiceFactory()
        {
            var fileSystemAccessor = new FileSystemIOAccessor();
            if (fileSystemAccessor.IsFileExists(dbFileName))
                fileSystemAccessor.DeleteFile(dbFileName);
            return new SqliteServiceFactory(fileSystemAccessor);
        }

        protected static HeaderStructureForLevel CreateHeaderStructureForLevel(string levelName = "table name", string[] referenceNames = null, ValueVector<Guid> levelScopeVector=null)
        {
            return new HeaderStructureForLevel()
            {
                LevelScopeVector = levelScopeVector?? new ValueVector<Guid>(),
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

        protected static QuestionnaireExportStructure CreateQuestionnaireExportStructure(params HeaderStructureForLevel[] levels)
        {
            var header = new Dictionary<ValueVector<Guid>, HeaderStructureForLevel>();
            if (levels != null && levels.Length > 0)
            {
                header = levels.ToDictionary((i) => i.LevelScopeVector, (i) => i);
            }
            return new QuestionnaireExportStructure() { HeaderToLevelMap = header };
        }

        protected static InterviewDataExportView CreateInterviewDataExportView(Guid? interviewId = null, params InterviewDataExportLevelView[] levels)
        {
            return new InterviewDataExportView(interviewId??Guid.NewGuid(),Guid.NewGuid(),1, levels??new InterviewDataExportLevelView[0]);
        }

        protected static InterviewDataExportLevelView CreateInterviewDataExportLevelView(Guid? interviewId = null, string levelName = "main level", ValueVector<Guid> levelVector =null, InterviewDataExportRecord[] records=null)
        {
            var interviewIdNotNull = interviewId ?? Guid.NewGuid();
            return new InterviewDataExportLevelView(levelVector ?? new ValueVector<Guid>(), levelName,
                records ?? new[]
                {
                    CreateInterviewDataExportRecord(interviewIdNotNull)
                }, interviewIdNotNull.FormatGuid());
        }

        protected static InterviewDataExportRecord CreateInterviewDataExportRecord(Guid interviewId, string recordId=null, string[] referenceValues=null, string[] parentIds=null, ExportedQuestion[] questions=null)
        {
            return new InterviewDataExportRecord(interviewId, recordId ?? interviewId.FormatGuid(), referenceValues ?? new string[0],
                parentIds ?? new string[0],
                questions ?? new[] { CreateExportedQuestion(), CreateExportedQuestion(new[] { "1" }, QuestionType.Numeric) });
        }

        protected static ExportedQuestion CreateExportedQuestion(string[] answers = null, QuestionType questionType = QuestionType.Text)
        {
            return new ExportedQuestion() { Answers = answers ?? new[] { "a" }, QuestionType = questionType };
        }

        protected static Mock<IFileSystemAccessor> CreateIFileSystemAccessorMock()
        {
            var result = new Mock<IFileSystemAccessor>();
            result.Setup(x => x.GetFileName(It.IsAny<string>())).Returns("");
            result.Setup(x => x.CombinePath(It.IsAny<string>(), It.IsAny<string>())).Returns<string, string>(Path.Combine);
            return result;
        } 
        protected static ExportedHeaderItem CreateExportedHeaderItem(QuestionType type = QuestionType.Text, string[] columnNames=null)
        {
            return new ExportedHeaderItem() { ColumnNames = columnNames ?? new[] { "1" }, QuestionType = type };
        }

        private static string dbFileName = "data.sqlite";
        protected static string[][] QueryTo2DimensionStringArray(ISqlServiceFactory sqlServiceFactory, string sql)
        {
            var result = new List<string[]>();
            using (var sqlService = sqlServiceFactory.CreateSqlService(dbFileName))
            {
                IEnumerable<dynamic> queryResult = sqlService.Query(sql);

                foreach (var row in queryResult)
                {
                    var resultRow = new List<string>();

                    foreach (var cell in row)
                    {
                        var byteArrayAsCellValue = cell.Value as byte[];
                        if (byteArrayAsCellValue != null)
                        {
                            resultRow.Add(Encoding.Unicode.GetString(byteArrayAsCellValue));
                        }
                        else

                            resultRow.Add(cell.Value.ToString());
                    }

                    result.Add(resultRow.ToArray());
                }
            }
            return result.ToArray();
        }

        protected static void RunCommand(ISqlServiceFactory sqlServiceFactory, string sql)
        {
            using (var sqlService = sqlServiceFactory.CreateSqlService(dbFileName))
            {
                sqlService.ExecuteCommand(sql);
            }
        }
    }
}
