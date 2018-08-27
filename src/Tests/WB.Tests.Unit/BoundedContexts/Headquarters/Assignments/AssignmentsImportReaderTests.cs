using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Assignments
{
    [TestOf(typeof(AssignmentsImportReader))]
    internal class AssignmentsImportReaderTests
    {
        [Test]
        public void when_read_text_file_info_with_2_columns_should_return_specified_file_info_with_specified_columns()
        {
            // arrange
            var fileName = "text.txt";
            var columns = new[] { "column1", "column2" };

            var reader = Create.Service.AssignmentsImportReader();
            var stream = Create.Other.TabDelimitedTextStream(columns);
            // act
            var fileInfo = reader.ReadTextFileInfo(stream, fileName);
            // assert
            Assert.That(fileInfo.FileName, Is.EqualTo(fileName));
            Assert.That(fileInfo.QuestionnaireOrRosterName, Is.EqualTo("text"));
            Assert.That(fileInfo.Columns, Is.EquivalentTo(columns));
        }

        [Test]
        public void when_read_text_file_info_with_whitespaces_in_columns_should_return_specified_file_info_with_specified_columns_without_whitespaces()
        {
            // arrange
            var fileName = "text.txt";
            var columns = new[] { "column1 ", " column2" };

            var reader = Create.Service.AssignmentsImportReader();
            var stream = Create.Other.TabDelimitedTextStream(columns);
            // act
            var fileInfo = reader.ReadTextFileInfo(stream, fileName);
            // assert
            Assert.That(fileInfo.FileName, Is.EqualTo(fileName));
            Assert.That(fileInfo.QuestionnaireOrRosterName, Is.EqualTo("text"));
            Assert.That(fileInfo.Columns, Is.EquivalentTo(new[] {"column1", "column2"}));
        }

        [Test]
        public void when_read_zip_file_info_and_file_is_not_an_arhive_should_return_empty_file_infos()
        {
            // arrange
            var columns = new[] { "column1", "column2" };

            var reader = Create.Service.AssignmentsImportReader();
            var stream = Create.Other.TabDelimitedTextStream(columns);
            // act
            var fileInfos = reader.ReadZipFileInfo(stream);
            // assert
            Assert.That(fileInfos, Is.Empty);
        }

        [Test]
        public void when_read_zip_file_and_file_is_not_an_arhive_should_return_empty_file_infos()
        {
            // arrange
            var columns = new[] { "column1", "column2" };

            var reader = Create.Service.AssignmentsImportReader();
            var stream = Create.Other.TabDelimitedTextStream(columns);
            // act
            var files = reader.ReadZipFile(stream);
            // assert
            Assert.That(files, Is.Empty);
        }

        [Test]
        public void when_read_file_from_zip_file_and_zip_not_an_arhive_should_return_null()
        {
            // arrange
            var columns = new[] { "column1", "column2" };

            var reader = Create.Service.AssignmentsImportReader();
            var stream = Create.Other.TabDelimitedTextStream(columns);
            // act
            var files = reader.ReadFileFromZip(stream, "file.tab");
            // assert
            Assert.That(files, Is.Null);
        }

        [Test]
        public void when_read_text_file_with_2_columns_should_return_preloaded_file_with_specified_columns()
        {
            // arrange
            var fileName = "text.txt";
            var columns = new[] { "column1", "column2" };

            var reader = Create.Service.AssignmentsImportReader();
            var stream = Create.Other.TabDelimitedTextStream(columns);
            // act
            var file = reader.ReadTextFile(stream, fileName);
            // assert
            Assert.That(file.FileInfo.FileName, Is.EqualTo(fileName));
            Assert.That(file.FileInfo.QuestionnaireOrRosterName, Is.EqualTo("text"));
            Assert.That(file.FileInfo.Columns, Is.EquivalentTo(columns));
        }

        [Test]
        public void when_read_text_file_with_columns_which_have_whitespaces_should_return_preloaded_file_with_columns_without_whitespaces()
        {
            // arrange
            var fileName = "text.txt";
            var columns = new[] { " column1", "column2 " };

            var reader = Create.Service.AssignmentsImportReader();
            var stream = Create.Other.TabDelimitedTextStream(columns);
            // act
            var file = reader.ReadTextFile(stream, fileName);
            // assert
            Assert.That(file.FileInfo.FileName, Is.EqualTo(fileName));
            Assert.That(file.FileInfo.QuestionnaireOrRosterName, Is.EqualTo("text"));
            Assert.That(file.FileInfo.Columns, Is.EquivalentTo(new[] {"column1", "column2"}));
        }

        [Test]
        public void when_read_text_file_with_columns_which_have_whitespaces_should_return_preloaded_file_with_row_with_cells_which_have_variable_names_without_whitespaces()
        {
            // arrange
            var fileName = "text.txt";
            var columns = new[] { " column1", "column2 " };

            var reader = Create.Service.AssignmentsImportReader();
            var stream = Create.Other.TabDelimitedTextStream(columns, new []{ "1", "2" });
            // act
            var file = reader.ReadTextFile(stream, fileName);
            // assert
            Assert.That(file.Rows[0].Cells[0].VariableOrCodeOrPropertyName, Is.EqualTo("column1"));
            Assert.That(file.Rows[0].Cells[1].VariableOrCodeOrPropertyName, Is.EqualTo("column2"));
            Assert.That(((PreloadingValue)file.Rows[0].Cells[0]).Column, Is.EqualTo("column1"));
            Assert.That(((PreloadingValue)file.Rows[0].Cells[1]).Column, Is.EqualTo("column2"));
        }

        [Test]
        public void when_read_text_file_with_1_row_and_interview_id_column_should_return_preloaded_file_with_specified_preloding_value()
        {
            // arrange
            var interviewId = "interview1";
            var columns = new[] {"interview__Id"};
            var row = new[] {interviewId};

            var reader = Create.Service.AssignmentsImportReader();
            var stream = Create.Other.TabDelimitedTextStream(columns, row);
            // act
            var file = reader.ReadTextFile(stream, "file.tab");
            // assert
            Assert.That(file.Rows[0].Cells, Is.EquivalentTo(new[]
            {
                new PreloadingValue
                {
                    VariableOrCodeOrPropertyName = "interview__id",
                    Column = columns[0],
                    Row = 1,
                    Value = interviewId
                }
            }).Using(new PreloadingValueComparer()));
        }

        [Test]
        public void when_read_text_file_with_1_row_and_2_roster_instance_codes_should_return_preloaded_file_with_2_specified_preloding_values()
        {
            // arrange
            var columns = new[] { "roster__Id", "nestedroster__ID" };
            var row = new[] { "1", "2" };

            var reader = Create.Service.AssignmentsImportReader();
            var stream = Create.Other.TabDelimitedTextStream(columns, row);
            // act
            var file = reader.ReadTextFile(stream, "file.tab");
            // assert
            Assert.That(file.Rows[0].Cells, Is.EquivalentTo(new[]
            {
                new PreloadingValue
                {
                    VariableOrCodeOrPropertyName = columns[0].ToLower(),
                    Column = columns[0],
                    Row = 1,
                    Value = row[0]
                },
                new PreloadingValue
                {
                    VariableOrCodeOrPropertyName = columns[1].ToLower(),
                    Column = columns[1],
                    Row = 1,
                    Value = row[1]
                }
            }).Using(new PreloadingValueComparer()));
        }

        [TestCase("SsSys_IRnd")]
        [TestCase("has__Errors")]
        [TestCase("interview__Key")]
        [TestCase("interview__Status")]
        public void when_read_text_file_with_1_row_and_system_column_should_return_preloaded_file_with_empty_preloading_values(string systemVariable)
        {
            // arrange
            var columns = new[] { systemVariable };
            var row = new[] { "1" };

            var reader = Create.Service.AssignmentsImportReader();
            var stream = Create.Other.TabDelimitedTextStream(columns, row);
            // act
            var file = reader.ReadTextFile(stream, "file.tab");
            // assert
            Assert.That(file.Rows[0].Cells, Is.Empty);
        }

        [Test]
        public void when_read_text_file_with_1_row_and_value_by_question_or_variable_should_return_preloaded_file_with_1_specified_preloding_value()
        {
            // arrange
            var value = "1";
            var columns = new[] { "SINGLE" };
            var row = new[] { value };

            var reader = Create.Service.AssignmentsImportReader();
            var stream = Create.Other.TabDelimitedTextStream(columns, row);
            // act
            var file = reader.ReadTextFile(stream, "file.tab");
            // assert
            Assert.That(file.Rows[0].Cells, Is.EquivalentTo(new[]
            {
                new PreloadingValue
                {
                    VariableOrCodeOrPropertyName = columns[0].ToLower(),
                    Column = columns[0],
                    Row = 1,
                    Value = value
                }
            }).Using(new PreloadingValueComparer()));
        }

        [Test]
        public void when_read_text_file_with_1_row_and_answers_by_composite_columns_should_return_preloaded_file_with_1_specified_composite_preloding_value_with_2_preloading_values()
        {
            // arrange
            var variable = "MULTI";
            var columns = new[] { $"{variable}__10", $"{variable}__20" };
            var row = new[] { "1", "2" };

            var reader = Create.Service.AssignmentsImportReader();
            var stream = Create.Other.TabDelimitedTextStream(columns, row);
            // act
            var file = reader.ReadTextFile(stream, "file.tab");
            // assert
            Assert.That(file.Rows[0].Cells[0], Is.InstanceOf<PreloadingCompositeValue>());
            Assert.That(file.Rows[0].Cells[0].VariableOrCodeOrPropertyName, Is.EqualTo(variable.ToLower()));
            Assert.That(((PreloadingCompositeValue) file.Rows[0].Cells[0]).Values, Is.EquivalentTo(new[]
            {
                new PreloadingValue
                {
                    VariableOrCodeOrPropertyName = "10",
                    Column = columns[0],
                    Row = 1,
                    Value = "1"
                },
                new PreloadingValue
                {
                    VariableOrCodeOrPropertyName = "20",
                    Column = columns[1],
                    Row = 1,
                    Value = "2"
                }
            }).Using(new PreloadingValueComparer()));
        }

        [Test]
        public void when_read_text_file_with_1_row_and_value_has_MissingStringQuestionValue_should_return_preloaded_file_with_1_specified_preloding_value()
        {
            // arrange
            var value = "##N/A##";
            var columns = new[] { "SINGLE" };
            var row = new[] { value };

            var reader = Create.Service.AssignmentsImportReader();
            var stream = Create.Other.TabDelimitedTextStream(columns, row);
            // act
            var file = reader.ReadTextFile(stream, "file.tab");
            // assert
            Assert.That(((PreloadingValue)file.Rows[0].Cells[0]).Value, Is.EqualTo(string.Empty));
        }

        [Test]
        public void when_read_text_file_with_1_row_and_value_has_MissingNumericQuestionValue_should_return_preloaded_file_with_1_specified_preloding_value()
        {
            // arrange
            var value = "-999999999";
            var columns = new[] { "SINGLE" };
            var row = new[] { value };

            var reader = Create.Service.AssignmentsImportReader();
            var stream = Create.Other.TabDelimitedTextStream(columns, row);
            // act
            var file = reader.ReadTextFile(stream, "file.tab");
            // assert
            Assert.That(((PreloadingValue)file.Rows[0].Cells[0]).Value, Is.EqualTo(string.Empty));
        }

        [Test]
        public void when_read_text_file_with_1_row_and_value_has_MissingQuantityValue_should_return_preloaded_file_with_1_specified_preloding_value()
        {
            // arrange
            var value = "INF";
            var columns = new[] { "SINGLE" };
            var row = new[] { value };

            var reader = Create.Service.AssignmentsImportReader();
            var stream = Create.Other.TabDelimitedTextStream(columns, row);
            // act
            var file = reader.ReadTextFile(stream, "file.tab");
            // assert
            Assert.That(((PreloadingValue)file.Rows[0].Cells[0]).Value, Is.EqualTo("-1"));
        }

        [Test]
        public void when_read_text_file_with_1_row_and_1_answer_by_composite_columns_should_return_preloaded_file_with_1_specified_composite_preloding_value_with_1_preloading_value()
        {
            // arrange
            var variable = "MULTI";
            var columns = new[] { $"{variable}__10" };
            var row = new[] { "1" };

            var reader = Create.Service.AssignmentsImportReader();
            var stream = Create.Other.TabDelimitedTextStream(columns, row);
            // act
            var file = reader.ReadTextFile(stream, "file.tab");
            // assert
            Assert.That(file.Rows[0].Cells[0], Is.InstanceOf<PreloadingCompositeValue>());
            Assert.That(file.Rows[0].Cells[0].VariableOrCodeOrPropertyName, Is.EqualTo(variable.ToLower()));
            Assert.That(((PreloadingCompositeValue)file.Rows[0].Cells[0]).Values, Is.EquivalentTo(new[]
            {
                new PreloadingValue
                {
                    VariableOrCodeOrPropertyName = "10",
                    Column = columns[0],
                    Row = 1,
                    Value = row[0]
                }
            }).Using(new PreloadingValueComparer()));
        }

        [Test]
        public void when_read_protected_variables_file_should_return_preloaded_file_with_2_specified_preloding_values()
        {
            // arrange
            var variableName1 = "categorical";
            var variableName2 = "multi";
            var columns = new[] { "variable__Name" };
            var rows = new[]
            {
                new[] {variableName1},
                new[] {variableName2}
            };

            var reader = Create.Service.AssignmentsImportReader();
            var stream = Create.Other.TabDelimitedTextStream(columns, rows);
            // act
            var file = reader.ReadTextFile(stream, "protected__variables.tab");
            // assert
            Assert.That(file.Rows, Has.Exactly(2).Items);
            Assert.That(file.Rows[0].Cells, Is.EquivalentTo(new[]
            {
                new PreloadingValue
                {
                    VariableOrCodeOrPropertyName = columns[0].ToLower(),
                    Column = columns[0],
                    Row = 1,
                    Value = variableName1
                }
            }).Using(new PreloadingValueComparer()));

            Assert.That(file.Rows[1].Cells, Is.EquivalentTo(new[]
            {
                new PreloadingValue
                {
                    VariableOrCodeOrPropertyName = columns[0].ToLower(),
                    Column = columns[0],
                    Row = 2,
                    Value = variableName2
                }
            }).Using(new PreloadingValueComparer()));
        }

        [Test]
        public void when_read_text_file_with_value_with_whitespaces_should_return_value_without_whitespaces()
        {
            // arrange
            var value = " value ";
            var columns = new[] { "column" };
            var row = new[] { value };

            var reader = Create.Service.AssignmentsImportReader();
            var stream = Create.Other.TabDelimitedTextStream(columns, row);
            // act
            var file = reader.ReadTextFile(stream, "file.tab");
            // assert
            Assert.That(((PreloadingValue)file.Rows[0].Cells[0]).Value, Is.EqualTo("value"));
        }

        private class PreloadingValueComparer : IEqualityComparer<PreloadingValue>
        {
            public bool Equals(PreloadingValue x, PreloadingValue y)
                => x.Value == y.Value && x.Column == y.Column && x.Row == y.Row &&
                   x.VariableOrCodeOrPropertyName == y.VariableOrCodeOrPropertyName;

            public int GetHashCode(PreloadingValue obj)
                => obj.Value.GetHashCode() ^ obj.Column.GetHashCode() ^ obj.Row ^
                   obj.VariableOrCodeOrPropertyName.GetHashCode();
        }
    }
}
