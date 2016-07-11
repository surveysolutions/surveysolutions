using System;
using System.Collections.Generic;
using System.IO;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Moq;
using OfficeOpenXml;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Core.SharedKernels.QuestionnaireEntities;
using It = Machine.Specifications.It;
using TranslationInstance = WB.Core.BoundedContexts.Designer.Translations.TranslationInstance;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.TranslationServiceTests
{
    internal class when_getting_translation_file_for_one_question : TranslationsServiceTestsContext
    {
        Establish context = () =>
        {
            questionnaireId = Guid.Parse("11111111111111111111111111111111");
            questionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            questionId1 = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");

            var storedTranslations = new List<TranslationInstance>
            {
                Create.TranslationInstance(type: TranslationType.Title,
                    translation: "title",
                    culture: culture,
                    questionnaireId: questionnaireId,
                    questionnaireEntityId: questionId),
                Create.TranslationInstance(type: TranslationType.Instruction,
                    translation: "instruction",
                    questionnaireId: questionnaireId,
                    culture: culture,
                    questionnaireEntityId: questionId),
                Create.TranslationInstance(type: TranslationType.OptionTitle,
                    translation: "translated option",
                    questionnaireId: questionnaireId,
                    culture: culture,
                    questionnaireEntityId: questionId,
                    translationIndex:"2"),
                Create.TranslationInstance(type: TranslationType.ValidationMessage,
                    translation: "validation message",
                    questionnaireId: questionnaireId,
                    culture: culture,
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
            
            var translationsStorage = new TestPlainStorage<TranslationInstance>();
            foreach (var translationInstance in storedTranslations)
            {
                translationsStorage.Store(translationInstance, translationInstance);
            }

            var questionnaires = new Mock<IReadSideKeyValueStorage<QuestionnaireDocument>>();
            questionnaires.SetReturnsDefault(questionnaire);

            service = CreateTranslationsService(translationsStorage, questionnaires.Object);
        };

        Because of = () =>
        {
            excelFileBytes = service.GetAsExcelFile(questionnaireId, culture);
            var memory = new MemoryStream(excelFileBytes);
            package  = new ExcelPackage(memory);
            cells = package.Workbook.Worksheets[1].Cells;
        };

        It should_output_question_title_translation = () =>
        {
            var questionTitleRow = 3;
            ((TranslationType)cells[questionTitleRow, translationTypeColumn].GetValue<int>()).ShouldEqual(TranslationType.Title);
            cells[questionTitleRow, translationIndexColumn].GetValue<string>().ShouldBeNull();
            cells[questionTitleRow, questionnaireEntityIdColumn].GetValue<string>().ShouldEqual(questionId.ToString());
            cells[questionTitleRow, originalTextColumn].GetValue<string>().ShouldEqual("non translated title");
            cells[questionTitleRow, translactionColumn].GetValue<string>().ShouldEqual("title");
        };

        It should_output_question_instructions_translation = () =>
        {
            var questionInstuctionsRow = 5;
            ((TranslationType)cells[questionInstuctionsRow, translationTypeColumn].GetValue<int>()).ShouldEqual(TranslationType.Instruction);
            cells[questionInstuctionsRow, translationIndexColumn].GetValue<string>().ShouldBeNull();
            cells[questionInstuctionsRow, questionnaireEntityIdColumn].GetValue<string>().ShouldEqual(questionId.ToString());
            cells[questionInstuctionsRow, originalTextColumn].GetValue<string>().ShouldEqual("non translated instruction");
            cells[questionInstuctionsRow, translactionColumn].GetValue<string>().ShouldEqual("instruction");
        };

        It should_output_question_validation_translation = () =>
        {
            var questionInstuctionsRow = 4;
            ((TranslationType)cells[questionInstuctionsRow, translationTypeColumn].GetValue<int>()).ShouldEqual(TranslationType.ValidationMessage);
            cells[questionInstuctionsRow, translationIndexColumn].GetValue<string>().ShouldEqual("1");
            cells[questionInstuctionsRow, questionnaireEntityIdColumn].GetValue<string>().ShouldEqual(questionId.ToString());
            cells[questionInstuctionsRow, originalTextColumn].GetValue<string>().ShouldEqual("non translated validation");
            cells[questionInstuctionsRow, translactionColumn].GetValue<string>().ShouldEqual("validation message");
        };

        It should_output_question_options_translation = () =>
        {
            var questionInstuctionsRow = 6;
            ((TranslationType)cells[questionInstuctionsRow, translationTypeColumn].GetValue<int>()).ShouldEqual(TranslationType.OptionTitle);
            cells[questionInstuctionsRow, translationIndexColumn].GetValue<string>().ShouldEqual("2");
            cells[questionInstuctionsRow, questionnaireEntityIdColumn].GetValue<string>().ShouldEqual(questionId.ToString());
            cells[questionInstuctionsRow, originalTextColumn].GetValue<string>().ShouldEqual("non translated option");
            cells[questionInstuctionsRow, translactionColumn].GetValue<string>().ShouldEqual("translated option");
        };

        It should_output_empty_translation_row_for_missing_translation_title = () =>
        {
            var questionTitleRow = 7;
            ((TranslationType)cells[questionTitleRow, translationTypeColumn].GetValue<int>()).ShouldEqual(TranslationType.Title);
            cells[questionTitleRow, translationIndexColumn].GetValue<string>().ShouldBeNull();
            cells[questionTitleRow, questionnaireEntityIdColumn].GetValue<string>().ShouldEqual(questionId1.ToString());
            cells[questionTitleRow, originalTextColumn].GetValue<string>().ShouldEqual("non translated title1");
            cells[questionTitleRow, translactionColumn].GetValue<string>().ShouldBeNull();
        };

        It should_output_empty_translation_row_for_missing_translation_instruction = () =>
        {
            var questionTitleRow = 9;
            ((TranslationType)cells[questionTitleRow, translationTypeColumn].GetValue<int>()).ShouldEqual(TranslationType.Instruction);
            cells[questionTitleRow, translationIndexColumn].GetValue<string>().ShouldBeNull();
            cells[questionTitleRow, questionnaireEntityIdColumn].GetValue<string>().ShouldEqual(questionId1.ToString());
            cells[questionTitleRow, originalTextColumn].GetValue<string>().ShouldEqual("non translated instruction 1");
            cells[questionTitleRow, translactionColumn].GetValue<string>().ShouldBeNull();
        };

        It should_output_empty_translation_row_for_missing_translation_validation_message = () =>
        {
            var questionTitleRow = 8;
            ((TranslationType)cells[questionTitleRow, translationTypeColumn].GetValue<int>()).ShouldEqual(TranslationType.ValidationMessage);
            cells[questionTitleRow, translationIndexColumn].GetValue<string>().ShouldEqual("1");
            cells[questionTitleRow, questionnaireEntityIdColumn].GetValue<string>().ShouldEqual(questionId1.ToString());
            cells[questionTitleRow, originalTextColumn].GetValue<string>().ShouldEqual("non translated validation 1");
            cells[questionTitleRow, translactionColumn].GetValue<string>().ShouldBeNull();
        };

        It should_output_empty_translation_row_for_missing_translation_option = () =>
        {
            var questionTitleRow = 10;
            ((TranslationType)cells[questionTitleRow, translationTypeColumn].GetValue<int>()).ShouldEqual(TranslationType.OptionTitle);
            cells[questionTitleRow, translationIndexColumn].GetValue<string>().ShouldEqual("1");
            cells[questionTitleRow, questionnaireEntityIdColumn].GetValue<string>().ShouldEqual(questionId1.ToString());
            cells[questionTitleRow, originalTextColumn].GetValue<string>().ShouldEqual("non translated option 1");
            cells[questionTitleRow, translactionColumn].GetValue<string>().ShouldBeNull();
        };

        Cleanup things = () => package?.Dispose();

        static Guid questionId;
        static TranslationsService service;
        static Guid questionId1;
        static Guid questionnaireId;
        static string culture = "en-US";
        static byte[] excelFileBytes;
        static ExcelPackage package;
        static ExcelRange cells;
    }
}