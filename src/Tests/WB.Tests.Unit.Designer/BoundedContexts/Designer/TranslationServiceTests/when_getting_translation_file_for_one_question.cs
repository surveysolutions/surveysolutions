using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using ClosedXML.Graphics;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Moq;
using NUnit.Framework;
using SixLabors.Fonts;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Abc;
using TranslationInstance = WB.Core.BoundedContexts.Designer.Translations.TranslationInstance;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.TranslationServiceTests
{
    internal class when_getting_translation_file_for_one_question : TranslationsServiceTestsContext
    {
        [SetUp]
        public void context()
        {
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

            QuestionnaireDocument questionnaire = Create.OldQuestionnaireDocumentWithOneChapter(
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
                },
                questionnaireId: questionnaireId);

            var translationsStorage = Create.InMemoryDbContext();
            translationsStorage.AddRange(storedTranslations);
            translationsStorage.SaveChanges();

            var questionnaires = new Mock<IQuestionnaireViewFactory>();
            questionnaires.SetReturnsDefault(Create.QuestionnaireView(questionnaire));

            service = Create.TranslationsService(translationsStorage, questionnaireStorage: questionnaires.Object);
            BecauseOf();
        }

        private void BecauseOf()
        {
            //non windows fonts
            var firstFont = SystemFonts.Collection.Families.First();
            var loadOptions = new LoadOptions { GraphicEngine = new DefaultGraphicEngine(firstFont.Name) };
            
            excelFile = service.GetAsExcelFile(new QuestionnaireRevision(questionnaireId), translationId);
            workbook = new XLWorkbook(new MemoryStream(excelFile.ContentAsExcelFile), loadOptions);
            worksheet = workbook.Worksheets.First();
        }

        [NUnit.Framework.Test]
        public void should_output_question_title_translation()
        {
            var questionTitleRow = 4;
            ((TranslationType)Enum.Parse(typeof(TranslationType), worksheet.Cell(questionTitleRow, translationTypeColumn).GetString())).Should().Be(TranslationType.Title);
            worksheet.Cell(questionTitleRow, translationIndexColumn).Value.ToString().Should().BeEmpty();
            worksheet.Cell(questionTitleRow, questionnaireEntityIdColumn).Value.ToString().Should().Be(questionId.FormatGuid());
            worksheet.Cell(questionTitleRow, originalTextColumn).Value.ToString().Should().Be("non translated title");
            worksheet.Cell(questionTitleRow, translactionColumn).Value.ToString().Should().Be("title");
        }

        [NUnit.Framework.Test]
        public void should_output_question_instructions_translation()
        {
            var questionInstuctionsRow = 6;
            ((TranslationType)Enum.Parse(typeof(TranslationType), worksheet.Cell(questionInstuctionsRow, translationTypeColumn).GetString())).Should().Be(TranslationType.Instruction);
            worksheet.Cell(questionInstuctionsRow, translationIndexColumn).Value.ToString().Should().BeEmpty();
            worksheet.Cell(questionInstuctionsRow, questionnaireEntityIdColumn).Value.ToString().Should().Be(questionId.FormatGuid());
            worksheet.Cell(questionInstuctionsRow, originalTextColumn).Value.ToString().Should().Be("non translated instruction");
            worksheet.Cell(questionInstuctionsRow, translactionColumn).Value.ToString().Should().Be("instruction");
        }

        [NUnit.Framework.Test]
        public void should_output_question_validation_translation()
        {
            var questionInstructionsRow = 5;
            ((TranslationType)Enum.Parse(typeof(TranslationType), worksheet.Cell(questionInstructionsRow, translationTypeColumn).GetString())).Should().Be(TranslationType.ValidationMessage);
            worksheet.Cell(questionInstructionsRow, translationIndexColumn).Value.ToString().Should().Be("1");
            worksheet.Cell(questionInstructionsRow, questionnaireEntityIdColumn).Value.ToString().Should().Be(questionId.FormatGuid());
            worksheet.Cell(questionInstructionsRow, originalTextColumn).Value.ToString().Should().Be("non translated validation");
            worksheet.Cell(questionInstructionsRow, translactionColumn).Value.ToString().Should().Be("validation message");
        }

        [NUnit.Framework.Test]
        public void should_output_question_options_translation()
        {
            var questionInstructionsRow = 7;
            ((TranslationType)Enum.Parse(typeof(TranslationType), worksheet.Cell(questionInstructionsRow, translationTypeColumn).GetString())).Should().Be(TranslationType.OptionTitle);
            worksheet.Cell(questionInstructionsRow, translationIndexColumn).Value.ToString().Should().Be("2");
            worksheet.Cell(questionInstructionsRow, questionnaireEntityIdColumn).Value.ToString().Should().Be(questionId.FormatGuid());
            worksheet.Cell(questionInstructionsRow, originalTextColumn).Value.ToString().Should().Be("non translated option");
            worksheet.Cell(questionInstructionsRow, translactionColumn).Value.ToString().Should().Be("translated option");
        }

        [NUnit.Framework.Test]
        public void should_output_empty_translation_row_for_missing_translation_title()
        {
            var questionTitleRow = 8;
            ((TranslationType)Enum.Parse(typeof(TranslationType), worksheet.Cell(questionTitleRow, translationTypeColumn).GetString())).Should().Be(TranslationType.Title);
            worksheet.Cell(questionTitleRow, translationIndexColumn).Value.ToString().Should().BeEmpty();
            worksheet.Cell(questionTitleRow, questionnaireEntityIdColumn).Value.ToString().Should().Be(questionId1.FormatGuid());
            worksheet.Cell(questionTitleRow, originalTextColumn).Value.ToString().Should().Be("non translated title1");
            worksheet.Cell(questionTitleRow, translactionColumn).Value.ToString().Should().BeEmpty();
        }

        [NUnit.Framework.Test]
        public void should_output_empty_translation_row_for_missing_translation_instruction()
        {
            var questionTitleRow = 10;
            ((TranslationType)Enum.Parse(typeof(TranslationType), worksheet.Cell(questionTitleRow, translationTypeColumn).GetString())).Should().Be(TranslationType.Instruction);
            worksheet.Cell(questionTitleRow, translationIndexColumn).Value.ToString().Should().BeEmpty();
            worksheet.Cell(questionTitleRow, questionnaireEntityIdColumn).Value.ToString().Should().Be(questionId1.FormatGuid());
            worksheet.Cell(questionTitleRow, originalTextColumn).Value.ToString().Should().Be("non translated instruction 1");
            worksheet.Cell(questionTitleRow, translactionColumn).Value.ToString().Should().BeEmpty();
        }

        [NUnit.Framework.Test]
        public void should_output_empty_translation_row_for_missing_translation_validation_message()
        {
            var questionTitleRow = 9;
            ((TranslationType)Enum.Parse(typeof(TranslationType), worksheet.Cell(questionTitleRow, translationTypeColumn).GetString())).Should().Be(TranslationType.ValidationMessage);
            worksheet.Cell(questionTitleRow, translationIndexColumn).Value.ToString().Should().Be("1");
            worksheet.Cell(questionTitleRow, questionnaireEntityIdColumn).Value.ToString().Should().Be(questionId1.FormatGuid());
            worksheet.Cell(questionTitleRow, originalTextColumn).Value.ToString().Should().Be("non translated validation 1");
            worksheet.Cell(questionTitleRow, translactionColumn).Value.ToString().Should().BeEmpty();
        }

        [NUnit.Framework.Test]
        public void should_output_empty_translation_row_for_missing_translation_option()
        {
            var questionTitleRow = 11;
            ((TranslationType)Enum.Parse(typeof(TranslationType), worksheet.Cell(questionTitleRow, translationTypeColumn).GetString())).Should().Be(TranslationType.OptionTitle);
            worksheet.Cell(questionTitleRow, translationIndexColumn).Value.ToString().Should().Be("1");
            worksheet.Cell(questionTitleRow, questionnaireEntityIdColumn).Value.ToString().Should().Be(questionId1.FormatGuid());
            worksheet.Cell(questionTitleRow, originalTextColumn).Value.ToString().Should().Be("non translated option 1");
            worksheet.Cell(questionTitleRow, translactionColumn).Value.ToString().Should().BeEmpty();
        }

        [TearDown]
        public void TearDown()
        {
            workbook?.Dispose();
        }

        Guid questionId;
        TranslationsService service;
        Guid questionId1;
        Guid questionnaireId;
        private readonly Guid translationId = Id.gD;
        TranslationFile excelFile;
        IXLWorksheet worksheet;
        private XLWorkbook workbook;
    }
}
