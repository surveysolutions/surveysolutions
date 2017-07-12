using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Tests.Abc;
using It = Moq.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.DataExport.ReadSideToTabularFormatExportServiceTests
{
    [TestFixture]
    [TestOf(typeof(ReadSideToTabularFormatExportService))]
    internal class ReadSideToTabularFormatExportServiceTests : ReadSideToTabularFormatExportServiceTestContext
    {
        [Test]
        public void When_generating_description_file_Then_should_generate_it_with_data_about_questionnaire_and_files()
        {
            // arrange
            string description = null;
            IFileSystemAccessor fileSystemAccessor = Mock.Of<IFileSystemAccessor>();

            Mock.Get(fileSystemAccessor)
                .Setup(accessor => accessor.CombinePath(@"x:\", "description.txt"))
                .Returns(@"x:\description.txt");
            Mock.Get(fileSystemAccessor)
                .Setup(accessor => accessor.WriteAllText(@"x:\description.txt", It.IsAny<string>()))
                .Callback<string, string>((file, content) => description = content);

            var questionnaireExportStructure = CreateQuestionnaireExportStructure(levels: new[]
            {
                CreateHeaderStructureForLevel("questionnaire", headerItems: new []
                {
                    CreateExportedHeaderItem(variableName: "x"),
                    CreateExportedHeaderItem(variableName: "y"),
                }),
                CreateHeaderStructureForLevel("roster", levelScopeVector: ValueVector.Create(Guid.NewGuid()), headerItems: new []
                {
                    CreateExportedHeaderItem(variableName: "name"),
                    CreateExportedHeaderItem(variableName: "age"),
                }),
            });

            var exportService = Create.Service.ReadSideToTabularFormatExportService(
                fileSystemAccessor: fileSystemAccessor,
                questionnaireExportStructure: questionnaireExportStructure);

            // act
            exportService.GenerateDescriptionFile(Create.Entity.QuestionnaireIdentity(), @"x:\", ".xlsx");

            // assert
            Assert.That(description, Is.Not.Empty);
            var lines = description.Split(new [] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            CollectionAssert.AreEqual(lines.Skip(1), new[]
            {
                "questionnaire.xlsx",
                "x, y",
                "roster.xlsx",
                "name, age",
            });
        }
    }

    [Subject(typeof(ReadSideToTabularFormatExportService))]
    internal class ReadSideToTabularFormatExportServiceTestContext
    {
        protected static HeaderStructureForLevel CreateHeaderStructureForLevel(
            string levelName = "table name", string[] referenceNames = null, ValueVector<Guid> levelScopeVector = null,
            IEnumerable<ExportedHeaderItem> headerItems = null)
        {
            return new HeaderStructureForLevel
            {
                LevelScopeVector = levelScopeVector ?? new ValueVector<Guid>(),
                LevelName = levelName,
                LevelIdColumnName = "Id",
                IsTextListScope = referenceNames != null,
                ReferencedNames = referenceNames,
                HeaderItems = headerItems?.ToDictionary(item => item.PublicKey, item => item)
                    ?? new Dictionary<Guid, ExportedHeaderItem>
                    {
                        { Guid.NewGuid(), CreateExportedHeaderItem() },
                        { Guid.NewGuid(), CreateExportedHeaderItem(QuestionType.Numeric, new[] { "a" }) }
                    },
            };
        }

        protected static ExportedHeaderItem CreateExportedHeaderItem(
            QuestionType type = QuestionType.Text, string[] columnNames = null, string variableName = "varname")
            => new ExportedHeaderItem
            {
                PublicKey = Guid.NewGuid(),
                ColumnNames = columnNames ?? new[] { "1" },
                Titles = columnNames ?? new[] { "1" },
                QuestionType = type,
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