using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Abc;
using It = Moq.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.DataExport.ReadSideToTabularFormatExportServiceTests
{
    [TestFixture]
    [TestOf(typeof(ReadSideToTabularFormatExportService))]
    internal class ReadSideToTabularFormatExportServiceTests : ReadSideToTabularFormatExportServiceTestContext
    {
        [Test]
        public void when_creating_template_for_preloading_from_questionnaire_export_structure1()
        {
            //a
            Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
            long questionnaireVersion = 3;

            var questionnaireExportStructure = CreateQuestionnaireExportStructure(
                    questionnaireId,
                    questionnaireVersion,
                    CreateHeaderStructureForLevel("main_level", levelScopeVector: ValueVector.Create(new Guid[0])),
                    CreateHeaderStructureForLevel("first_roster_level", referenceNames: new[] { "r1", "r2" }, levelScopeVector: ValueVector.Create(Id.g1)),
                    CreateHeaderStructureForLevel("second_roster_level", referenceNames: new[] { "r1", "r2" }, levelScopeVector: ValueVector.Create(Id.g1, Id.g2)),
                    CreateHeaderStructureForLevel("third_roster_level", referenceNames: new[] { "r1", "r2" }, levelScopeVector: ValueVector.Create(Id.g1, Id.g2, Id.g3)));

            List<IEnumerable<string[]>> rows = new List<IEnumerable<string[]>>();
            Mock<ICsvWriter> csvWriterMock = new Mock<ICsvWriter>();

            csvWriterMock.Setup(
                    x => x.WriteData(It.IsAny<string>(), It.IsAny<IEnumerable<string[]>>(), It.IsAny<string>()))
                    .Callback<string, IEnumerable<string[]>, string>((filePath, data, delimiter) => { rows.Add(data); });

            ReadSideToTabularFormatExportService readSideToTabularFormatExportService = Create.Service.ReadSideToTabularFormatExportService(csvWriter: csvWriterMock.Object,
                    questionnaireExportStructure: questionnaireExportStructure);
            

            //aa
            readSideToTabularFormatExportService.CreateHeaderStructureForPreloadingForQuestionnaire(new QuestionnaireIdentity(questionnaireId, questionnaireVersion), "");
            
            //aaa
            Assert.That(rows.Count, Is.EqualTo(4));
            Assert.That(rows[0].First(), 
                Is.EqualTo(new object[]
                {
                    ServiceColumns.InterviewId, "1", "a", "long__var", "ssSys_IRnd", ServiceColumns.Key, ServiceColumns.HasAnyError, ServiceColumns.InterviewStatus
                }));

            Assert.That(rows[1].First(),
                Is.EqualTo(new string[]
                {
                    "first_roster_level__id", "r1", "r2", "1", "a", "long__var", ServiceColumns.InterviewId, ServiceColumns.Key
                }));

            Assert.That(rows[2].First(), 
                Is.EqualTo(new string[]
                {
                    "second_roster_level__id", "r1", "r2", "1", "a", "long__var", "first_roster_level__id", ServiceColumns.InterviewId, ServiceColumns.Key
                }));

            Assert.That(rows[3].First(),
                Is.EqualTo(new string[]
                {
                    "third_roster_level__id", "r1", "r2", "1", "a", "long__var", "second_roster_level__id", "first_roster_level__id", ServiceColumns.InterviewId, ServiceColumns.Key
                }));
        }
    }

    [NUnit.Framework.TestOf(typeof(ReadSideToTabularFormatExportService))]
    internal class ReadSideToTabularFormatExportServiceTestContext
    {
        protected static HeaderStructureForLevel CreateHeaderStructureForLevel(
            string levelName = "table name", string[] referenceNames = null, ValueVector<Guid> levelScopeVector = null,
            IEnumerable<IExportedHeaderItem> headerItems = null)
        {
            return new HeaderStructureForLevel
            {
                LevelScopeVector = levelScopeVector ?? new ValueVector<Guid>(),
                LevelName = levelName,
                LevelIdColumnName = (levelScopeVector == null || levelScopeVector.Length == 0) ? ServiceColumns.InterviewId : string.Format(ServiceColumns.IdSuffixFormat, levelName),
                IsTextListScope = referenceNames != null,
                ReferencedNames = referenceNames,
                HeaderItems = headerItems?.ToDictionary(item => item.PublicKey, item => item)
                    ?? new Dictionary<Guid, IExportedHeaderItem>
                    {
                        { Guid.NewGuid(), ExportedQuestionHeaderItem() },
                        { Guid.NewGuid(), ExportedQuestionHeaderItem(QuestionType.Numeric, new[] { "a" }) },
                        { Guid.NewGuid(), ExportedVariableHeaderItem(VariableType.LongInteger, new[] { "long__var" }) }
                    },
            };
        }

        protected static IExportedHeaderItem ExportedQuestionHeaderItem(
            QuestionType type = QuestionType.Text, string[] columnNames = null, string variableName = "varname")
            => new ExportedQuestionHeaderItem
            {
                PublicKey = Guid.NewGuid(),
                ColumnHeaders = columnNames?.Select(x => new HeaderColumn() { Name = x, Title = x}).ToList() ?? new List<HeaderColumn>(){new HeaderColumn(){Name = "1", Title = "1"}},
                QuestionType = type,
                VariableName = variableName,
            };

        protected static IExportedHeaderItem ExportedVariableHeaderItem(
            VariableType type = VariableType.String, string[] columnNames = null, string variableName = "varname")
            => new ExportedVariableHeaderItem
            {
                PublicKey = Guid.NewGuid(),
                ColumnHeaders = columnNames?.Select(x => new HeaderColumn() { Name = x, Title = x }).ToList() ?? new List<HeaderColumn>() { new HeaderColumn() { Name = "var__1", Title = "var__1" } },
                VariableType = type,
                VariableName = variableName,
            };

        protected static QuestionnaireExportStructure CreateQuestionnaireExportStructure(
            Guid? questionnaireId = null, long? version = null, params HeaderStructureForLevel[] levels)
        {
            var header = levels != null && levels.Length > 0
                ? levels.ToDictionary(i => i.LevelScopeVector, i => i)
                : new Dictionary<ValueVector<Guid>, HeaderStructureForLevel>();

            return new QuestionnaireExportStructure
            {
                HeaderToLevelMap = header,
                QuestionnaireId = questionnaireId ?? Guid.NewGuid(),
                Version = version ?? 777,
            };
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
