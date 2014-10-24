using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Services.Sql;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using It = Moq.It;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.ServiceTests.DataExport.SqlDataExportWriterTests
{
    [Subject(typeof(SqlDataExportWriter))]
    internal class SqlDataExportWriterTestContext
    {
        protected static SqlDataExportWriter CreateSqlDataExportWriter(ISqlService sqlService = null, IFileSystemAccessor fileSystemAccessor=null)
        {
            return new SqlDataExportWriter(fileSystemAccessor?? CreateIFileSystemAccessorMock().Object,
                Mock.Of<ISqlServiceFactory>(_ => _.CreateSqlService(
                    It.IsAny<string>()) == sqlService));
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
    }

    internal class SqlServiceTestable : ISqlService
    {
        private readonly Dictionary<string,string[]> tableWithColumns;

        public SqlServiceTestable(Dictionary<string, string[]> tableWithColumns=null)
        {
            this.tableWithColumns = tableWithColumns?? new Dictionary<string, string[]>();
        }

        private readonly List<string> commandsToExecute = new List<string>();

        public void Dispose() {}

        public List<string> CommandsToExecute { get { return commandsToExecute; } }

        public IEnumerable<dynamic> Query(string sql, object param = null)
        {
            commandsToExecute.Add(sql);
            return new dynamic[0];
        }

        public IEnumerable<T> Query<T>(string sql, object param = null) where T : class
        {
            commandsToExecute.Add(sql);

            if (sql.Contains("information_schema.tables"))
                return tableWithColumns.Keys.Select(k =>  k  as T).ToArray();

            if (sql.Contains("INFORMATION_SCHEMA.COLUMNS"))
            {
                string tableName = (param as dynamic).tableName;
                if (!tableWithColumns.ContainsKey(tableName))
                    return new T[0];
                return tableWithColumns[tableName].Select(c => c as T).ToArray();
            }

            return new T[0];
        }

        public void ExecuteCommand(string sql, object param = null)
        {
            commandsToExecute.Add(sql);
        }
    }
}
