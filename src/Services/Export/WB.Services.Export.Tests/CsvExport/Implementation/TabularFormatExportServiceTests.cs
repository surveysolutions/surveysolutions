﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WB.Services.Export.CsvExport.Implementation;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Questionnaire.Services;
using WB.Services.Export.Services;

namespace WB.Services.Export.Tests.CsvExport.Implementation
{
    [TestFixture]
    [TestOf(typeof(TabularFormatExportService))]
    internal class TabularFormatExportServiceTests
    {
        [Test]
        public async Task When_generating_description_file_Then_should_generate_it_with_data_about_questionnaire_and_files()
        {
            // arrange
            string description = null;
            var fileSystemAccessor = new Mock<IFileSystemAccessor>();
            fileSystemAccessor
                .Setup(accessor => accessor.WriteAllText(@"x:\export__readme.txt", It.IsAny<string>()))
                .Callback<string, string>((file, content) => description = content);

            var questionnaireStorage = new Mock<IQuestionnaireStorage>();
            var questionnaireDocument = Create.QuestionnaireDocument(Guid.Parse("11111111111111111111111111111111"), 555);
            questionnaireStorage.SetupIgnoreArgs(x => x.GetQuestionnaireAsync(null, CancellationToken.None))
                .ReturnsAsync(questionnaireDocument);

            var questionnaireExportStructure = CreateQuestionnaireExportStructure(levels: new[]
            {
                CreateHeaderStructureForLevel("questionnaire", headerItems: new[]
                {
                    ExportedQuestionHeaderItem(variableName: "x"),
                    ExportedQuestionHeaderItem(variableName: "y"),
                }),
                CreateHeaderStructureForLevel("roster", levelScopeVector: ValueVector.Create(Guid.NewGuid()),
                    headerItems: new[]
                    {
                        ExportedQuestionHeaderItem(variableName: "name"),
                        ExportedQuestionHeaderItem(variableName: "age"),
                    }),
            });

            var hqApi = new Mock<IHeadquartersApi>();

            var tenantApi = Create.TenantHeadquartersApi(hqApi.Object);

            var exportService = Create.ReadSideToTabularFormatExportService(
                questionnaireExportStructure,
                tenantApi,

                fileSystemAccessor: fileSystemAccessor.Object,
                questionnaireStorage: questionnaireStorage.Object);

            // act
            await exportService.GenerateDescriptionFileAsync(Create.Tenant(), new QuestionnaireId(questionnaireExportStructure.QuestionnaireId), @"x:\", ".xlsx");

            // assert
            Assert.That(description, Is.Not.Empty);
            var lines = description.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
            CollectionAssert.AreEqual(lines.Skip(3), new[]
            {
                "questionnaire.xlsx",
                "x, y",
                "roster.xlsx",
                "name, age",
            });
        }

         [Test]
        public async Task When_generating_description_file_Then_should_generate_link_to_view_questionnaire_on_designer()
        {
            // arrange
            string description = null;
            var fileSystemAccessor = new Mock<IFileSystemAccessor>();
            fileSystemAccessor
                .Setup(accessor => accessor.WriteAllText(@"x:\export__readme.txt", It.IsAny<string>()))
                .Callback<string, string>((file, content) => description = content);

            var questionnaireStorage = new Mock<IQuestionnaireStorage>();
            var questionnaireDocument = Create.QuestionnaireDocument();
            questionnaireDocument.Title = "Name of questionnaire";
            questionnaireDocument.Id = "11111111111111111111111111111111";
            questionnaireDocument.Revision = 1;
            questionnaireStorage.SetupIgnoreArgs(x => x.GetQuestionnaireAsync(null, CancellationToken.None))
                .ReturnsAsync(questionnaireDocument);

            var questionnaireExportStructure = CreateQuestionnaireExportStructure();
            var hqApi = new Mock<IHeadquartersApi>();
            var tenantApi = Create.TenantHeadquartersApi(hqApi.Object);

            var exportService = Create.ReadSideToTabularFormatExportService(
                questionnaireExportStructure,
                tenantApi,

                fileSystemAccessor: fileSystemAccessor.Object,
                questionnaireStorage: questionnaireStorage.Object);

            // act
            await exportService.GenerateDescriptionFileAsync(Create.Tenant(), new QuestionnaireId("id"), @"x:\", ".xlsx");

            // assert
            Assert.That(description, Is.Not.Empty);
            var lines = description.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
            Assert.That(lines[1], Is.EqualTo($"The data in this download were collected using the Survey Solutions questionnaire \"{questionnaireDocument.Title}\". "));
            Assert.That(lines[2], Is.EqualTo($"You can open the questionnaire in the Survey Solutions Designer online by that link: https://designer.mysurvey.solutions/questionnaire/details/11111111111111111111111111111111$1"));
        }

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
            string questionnaireId = null, params HeaderStructureForLevel[] levels)
        {
            var header = levels != null && levels.Length > 0
                ? levels.ToDictionary(i => i.LevelScopeVector, i => i)
                : new Dictionary<ValueVector<Guid>, HeaderStructureForLevel>();

            return new QuestionnaireExportStructure
            {
                HeaderToLevelMap = header,
                QuestionnaireId = questionnaireId 
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
