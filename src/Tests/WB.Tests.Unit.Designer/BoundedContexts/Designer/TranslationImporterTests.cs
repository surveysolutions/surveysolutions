using System;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using FluentAssertions;
using Main.Core.Documents;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Infrastructure.Native.Questionnaire;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer
{
    [TestFixture]
    [TestOf(typeof(TranslationImporter))]
    public class TranslationImporterTests
    {
        private static readonly Guid QuestionId = Guid.NewGuid();
        private static readonly Guid TranslationId = Guid.NewGuid();
        private static readonly QuestionnaireIdentity Identity = new QuestionnaireIdentity(Guid.NewGuid(), 1);

        private static QuestionnaireDocument Questionnaire()
            => Create.QuestionnaireDocumentWithOneChapter(Create.TextQuestion(QuestionId, variable: "q1"));

        private static byte[] Workbook(string sheetName, params object[][] rows)
        {
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet(sheetName);
            var headers = new[]
            {
                TranslationExcelOptions.EntityIdColumnName, TranslationExcelOptions.VariableColumnName,
                TranslationExcelOptions.TranslationTypeColumnName,
                TranslationExcelOptions.OptionValueOrValidationIndexOrFixedRosterIdIndexColumnName,
                "Original text", TranslationExcelOptions.TranslationTextColumnName
            };
            for (var c = 0; c < headers.Length; c++)
                ws.Cell(1, c + 1).Value = headers[c];
            for (var r = 0; r < rows.Length; r++)
                for (var c = 0; c < rows[r].Length; c++)
                    ws.Cell(r + 2, c + 1).Value = XLCellValue.FromObject(rows[r][c]);
            using var stream = new MemoryStream();
            wb.SaveAs(stream);
            return stream.ToArray();
        }

        private static object[] Row(Guid entityId, string type, string index, string translation)
            => new object[] { entityId.ToString(), "q1", type, index, "orig", translation };

        [Test]
        public void when_translated_title_row_should_create_translation_instance()
        {
            var bytes = Workbook(TranslationExcelOptions.WorksheetName, Row(QuestionId, "Title", "", "Titre"));

            var result = new TranslationImporter().GetTranslationInstancesFromExcelFile(Questionnaire(), Identity, TranslationId, bytes);

            var item = result.Should().ContainSingle().Subject;
            item.QuestionnaireEntityId.Should().Be(QuestionId);
            item.TranslationId.Should().Be(TranslationId);
            item.QuestionnaireId.Should().Be(Identity);
            item.Value.Should().Be("Titre");
            item.Type.Should().Be(TranslationType.Title);
            item.TranslationIndex.Should().BeNull();
        }

        [Test]
        public void when_translation_is_blank_should_skip_row()
        {
            var bytes = Workbook(TranslationExcelOptions.WorksheetName,
                Row(QuestionId, "Title", "", "  "),
                Row(QuestionId, "Instruction", "", "Consigne"));

            var result = new TranslationImporter().GetTranslationInstancesFromExcelFile(Questionnaire(), Identity, TranslationId, bytes);

            result.Should().ContainSingle().Which.Type.Should().Be(TranslationType.Instruction);
        }

        [Test]
        public void when_entity_is_not_in_questionnaire_should_ignore_row()
        {
            var bytes = Workbook(TranslationExcelOptions.WorksheetName, Row(Guid.NewGuid(), "Title", "", "Titre"));

            new TranslationImporter().GetTranslationInstancesFromExcelFile(Questionnaire(), Identity, TranslationId, bytes)
                .Should().BeEmpty();
        }

        [Test]
        public void when_index_has_trailing_dollar_should_trim_it()
        {
            var bytes = Workbook(TranslationExcelOptions.WorksheetName, Row(QuestionId, "ValidationMessage", "1$", "Invalide"));

            var result = new TranslationImporter().GetTranslationInstancesFromExcelFile(Questionnaire(), Identity, TranslationId, bytes);

            result.Single().TranslationIndex.Should().Be("1");
        }

        [Test]
        public void when_same_entity_type_and_index_repeated_should_keep_one()
        {
            var bytes = Workbook(TranslationExcelOptions.WorksheetName,
                Row(QuestionId, "Title", "", "First"),
                Row(QuestionId, "Title", "", "Second"));

            new TranslationImporter().GetTranslationInstancesFromExcelFile(Questionnaire(), Identity, TranslationId, bytes)
                .Should().ContainSingle();
        }

        [Test]
        public void when_options_sheet_present_should_be_read_too()
        {
            var bytes = Workbook(TranslationExcelOptions.OptionsWorksheetPreffix + "q1", Row(QuestionId, "OptionTitle", "5", "Cinq"));

            var result = new TranslationImporter().GetTranslationInstancesFromExcelFile(Questionnaire(), Identity, TranslationId, bytes);

            var item = result.Should().ContainSingle().Subject;
            item.Type.Should().Be(TranslationType.OptionTitle);
            item.TranslationIndex.Should().Be("5");
        }

        [Test]
        public void when_no_translation_sheet_should_throw()
        {
            var bytes = Workbook("Other", Row(QuestionId, "Title", "", "x"));

            Assert.Throws<InvalidOperationException>(() =>
                new TranslationImporter().GetTranslationInstancesFromExcelFile(Questionnaire(), Identity, TranslationId, bytes))!
                .Message.Should().Contain("missing");
        }

        [Test]
        public void when_file_is_not_a_workbook_should_throw_invalid_operation()
        {
            Assert.Throws<InvalidOperationException>(() =>
                new TranslationImporter().GetTranslationInstancesFromExcelFile(Questionnaire(), Identity, TranslationId, new byte[] { 1, 2, 3 }));
        }
    }
}

