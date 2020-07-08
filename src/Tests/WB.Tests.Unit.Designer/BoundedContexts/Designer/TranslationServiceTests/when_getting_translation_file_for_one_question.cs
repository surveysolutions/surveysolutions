using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Moq;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Core.SharedKernels.QuestionnaireEntities;
using TranslationInstance = WB.Core.BoundedContexts.Designer.Translations.TranslationInstance;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.TranslationServiceTests
{
    internal class when_getting_translation_file_for_one_question : TranslationsServiceTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaireId = Guid.Parse("11111111111111111111111111111111");
            questionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            questionId1 = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");

            var storedTranslations = new List<TranslationInstance>
            {
                Create.TranslationInstance(type: TranslationType.Title,
                    translation: "title",
                    translationId: translationId,
                    questionnaireId: questionnaireId,
                    questionnaireEntityId: questionId),
                Create.TranslationInstance(type: TranslationType.Instruction,
                    translation: "instruction",
                    questionnaireId: questionnaireId,
                    translationId: translationId,
                    questionnaireEntityId: questionId),
                Create.TranslationInstance(type: TranslationType.OptionTitle,
                    translation: "translated option",
                    questionnaireId: questionnaireId,
                    translationId: translationId,
                    questionnaireEntityId: questionId,
                    translationIndex:"2"),
                Create.TranslationInstance(type: TranslationType.ValidationMessage,
                    translation: "validation message",
                    questionnaireId: questionnaireId,
                    translationId: translationId,
                    questionnaireEntityId: questionId,
                    translationIndex:"1")
            };

            QuestionnaireDocument questionnaire = Create.QuestionnaireDocumentWithOneChapter(
                children: new IComposite[]
                {
                    Create.Question(questionId: questionId,
                        title: "non translated title",
                        instructions: "non translated instruction",
                        answers: Create.Answer("non translated option", 2),
                        validationConditions: new List<ValidationCondition>
                        {
                            Create.ValidationCondition(message: "non translated validation")
                        }),
                     Create.Question(questionId: questionId1,
                        title: "non translated title1",
                        instructions: "non translated instruction 1",
                        answers: Create.Answer("non translated option 1", 1),
                        validationConditions: new List<ValidationCondition>
                        {
                            Create.ValidationCondition(message: "non translated validation 1")
                        })
                });

            var translationsStorage = Create.InMemoryDbContext();
            translationsStorage.AddRange(storedTranslations);
            translationsStorage.SaveChanges();

            var questionnaires = new Mock<IPlainKeyValueStorage<QuestionnaireDocument>>();
            questionnaires.SetReturnsDefault(questionnaire);

            service = Create.TranslationsService(translationsStorage, questionnaireStorage: questionnaires.Object);
            BecauseOf();
        }

        private void BecauseOf() 
        {
            excelFile = service.GetAsExcelFile(questionnaireId, translationId);
            workbook = new XLWorkbook(new MemoryStream(excelFile.ContentAsExcelFile)).Worksheets.First();
        }

        [NUnit.Framework.Test] public void should_output_question_title_translation ()
        {
            var questionTitleRow = 4;
            ((TranslationType)Enum.Parse(typeof(TranslationType), workbook.Cell(questionTitleRow, translationTypeColumn).GetString())).Should().Be(TranslationType.Title);
            workbook.Cell(questionTitleRow, translationIndexColumn).Value?.ToString().Should().BeEmpty();
            workbook.Cell(questionTitleRow, questionnaireEntityIdColumn).Value?.ToString().Should().Be(questionId.FormatGuid());
            workbook.Cell(questionTitleRow, originalTextColumn).Value?.ToString().Should().Be("non translated title");
            workbook.Cell(questionTitleRow, translactionColumn).Value?.ToString().Should().Be("title");
        }

        [NUnit.Framework.Test] public void should_output_question_instructions_translation () 
        {
            var questionInstuctionsRow = 6;
            ((TranslationType)Enum.Parse(typeof(TranslationType), workbook.Cell(questionInstuctionsRow, translationTypeColumn).GetString())).Should().Be(TranslationType.Instruction);
            workbook.Cell(questionInstuctionsRow, translationIndexColumn).Value?.ToString().Should().BeEmpty();
            workbook.Cell(questionInstuctionsRow, questionnaireEntityIdColumn).Value?.ToString().Should().Be(questionId.FormatGuid());
            workbook.Cell(questionInstuctionsRow, originalTextColumn).Value?.ToString().Should().Be("non translated instruction");
            workbook.Cell(questionInstuctionsRow, translactionColumn).Value?.ToString().Should().Be("instruction");
        }

        [NUnit.Framework.Test] public void should_output_question_validation_translation () 
        {
            var questionInstuctionsRow = 5;
            ((TranslationType)Enum.Parse(typeof(TranslationType), workbook.Cell(questionInstuctionsRow, translationTypeColumn).GetString())).Should().Be(TranslationType.ValidationMessage);
            workbook.Cell(questionInstuctionsRow, translationIndexColumn).Value?.ToString().Should().Be("1");
            workbook.Cell(questionInstuctionsRow, questionnaireEntityIdColumn).Value?.ToString().Should().Be(questionId.FormatGuid());
            workbook.Cell(questionInstuctionsRow, originalTextColumn).Value?.ToString().Should().Be("non translated validation");
            workbook.Cell(questionInstuctionsRow, translactionColumn).Value?.ToString().Should().Be("validation message");
        }

        [NUnit.Framework.Test] public void should_output_question_options_translation () 
        {
            var questionInstuctionsRow = 7;
            ((TranslationType)Enum.Parse(typeof(TranslationType), workbook.Cell(questionInstuctionsRow, translationTypeColumn).GetString())).Should().Be(TranslationType.OptionTitle);
            workbook.Cell(questionInstuctionsRow, translationIndexColumn).Value?.ToString().Should().Be("2");
            workbook.Cell(questionInstuctionsRow, questionnaireEntityIdColumn).Value?.ToString().Should().Be(questionId.FormatGuid());
            workbook.Cell(questionInstuctionsRow, originalTextColumn).Value?.ToString().Should().Be("non translated option");
            workbook.Cell(questionInstuctionsRow, translactionColumn).Value?.ToString().Should().Be("translated option");
        }

        [NUnit.Framework.Test] public void should_output_empty_translation_row_for_missing_translation_title () 
        {
            var questionTitleRow = 8;
            ((TranslationType)Enum.Parse(typeof(TranslationType), workbook.Cell(questionTitleRow, translationTypeColumn).GetString())).Should().Be(TranslationType.Title);
            workbook.Cell(questionTitleRow, translationIndexColumn).Value?.ToString().Should().BeEmpty();
            workbook.Cell(questionTitleRow, questionnaireEntityIdColumn).Value?.ToString().Should().Be(questionId1.FormatGuid());
            workbook.Cell(questionTitleRow, originalTextColumn).Value?.ToString().Should().Be("non translated title1");
            workbook.Cell(questionTitleRow, translactionColumn).Value?.ToString().Should().BeEmpty();
        }

        [NUnit.Framework.Test] public void should_output_empty_translation_row_for_missing_translation_instruction () 
        {
            var questionTitleRow = 10;
            ((TranslationType)Enum.Parse(typeof(TranslationType), workbook.Cell(questionTitleRow, translationTypeColumn).GetString())).Should().Be(TranslationType.Instruction);
            workbook.Cell(questionTitleRow, translationIndexColumn).Value?.ToString().Should().BeEmpty();
            workbook.Cell(questionTitleRow, questionnaireEntityIdColumn).Value?.ToString().Should().Be(questionId1.FormatGuid());
            workbook.Cell(questionTitleRow, originalTextColumn).Value?.ToString().Should().Be("non translated instruction 1");
            workbook.Cell(questionTitleRow, translactionColumn).Value?.ToString().Should().BeEmpty();
        }

        [NUnit.Framework.Test] public void should_output_empty_translation_row_for_missing_translation_validation_message () 
        {
            var questionTitleRow = 9;
            ((TranslationType)Enum.Parse(typeof(TranslationType), workbook.Cell(questionTitleRow, translationTypeColumn).GetString())).Should().Be(TranslationType.ValidationMessage);
            workbook.Cell(questionTitleRow, translationIndexColumn).Value?.ToString().Should().Be("1");
            workbook.Cell(questionTitleRow, questionnaireEntityIdColumn).Value?.ToString().Should().Be(questionId1.FormatGuid());
            workbook.Cell(questionTitleRow, originalTextColumn).Value?.ToString().Should().Be("non translated validation 1");
            workbook.Cell(questionTitleRow, translactionColumn).Value?.ToString().Should().BeEmpty();
        }

        [NUnit.Framework.Test] public void should_output_empty_translation_row_for_missing_translation_option () 
        {
            var questionTitleRow = 11;
            ((TranslationType)Enum.Parse(typeof(TranslationType), workbook.Cell(questionTitleRow, translationTypeColumn).GetString())).Should().Be(TranslationType.OptionTitle);
            workbook.Cell(questionTitleRow, translationIndexColumn).Value?.ToString().Should().Be("1");
            workbook.Cell(questionTitleRow, questionnaireEntityIdColumn).Value?.ToString().Should().Be(questionId1.FormatGuid());
            workbook.Cell(questionTitleRow, originalTextColumn).Value?.ToString().Should().Be("non translated option 1");
            workbook.Cell(questionTitleRow, translactionColumn).Value?.ToString().Should().BeEmpty();
        }

        static Guid questionId;
        static TranslationsService service;
        static Guid questionId1;
        static Guid questionnaireId;
        static Guid translationId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        static TranslationFile excelFile;
        static IXLWorksheet workbook;
    }
}
