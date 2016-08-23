using System;
using System.Collections.Generic;
using System.IO;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using OfficeOpenXml;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.Questionnaire.Translations;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.TranslationServiceTests
{
    internal class when_getting_translation_file_for_questionnaire_with_combobox_and_cascading_question : TranslationsServiceTestsContext
    {
        Establish context = () =>
        {
            var storedTranslations = new List<TranslationInstance>
            {
                Create.TranslationInstance(type: TranslationType.OptionTitle,
                    translation: "Каскадная Опция",
                    translationIndex: "1",
                    translationId: translationId,
                    questionnaireId: questionnaireId,
                    questionnaireEntityId: cascadingQustionId),
                Create.TranslationInstance(type: TranslationType.OptionTitle,
                    translation: "Опция",
                    translationIndex: "1",
                    translationId: translationId,
                    questionnaireId: questionnaireId,
                    questionnaireEntityId: comboboxId),
            };

            QuestionnaireDocument questionnaire = Create.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.SingleQuestion(id: comboboxId, variable:"combobox", isFilteredCombobox: true, options: new List<Answer> {Create.Option("1", "Option")}),
                Create.SingleQuestion(id: cascadingQustionId, variable:"cascading", cascadeFromQuestionId: comboboxId, options: new List<Answer> {Create.Option("1", "Cascading Option", "1")})
            });

            var translationsStorage = new TestPlainStorage<TranslationInstance>();
            storedTranslations.ForEach(x => translationsStorage.Store(x, x));

            var questionnaires = new Mock<IReadSideKeyValueStorage<QuestionnaireDocument>>();
            questionnaires.SetReturnsDefault(questionnaire);

            service = Create.TranslationsService(translationsStorage, questionnaires.Object);
        };

        Because of = () =>
        {
            var excelFile = service.GetAsExcelFile(questionnaireId, translationId);
            var excelWorkbook = new ExcelPackage(new MemoryStream(excelFile.ContentAsExcelFile)).Workbook;
            comboboxCells = excelWorkbook.Worksheets["@@_combobox"].Cells;
            cascadingCells = excelWorkbook.Worksheets["@@_cascading"].Cells;
        };

        It should_export_translation_on__Translations_combobox__sheet_in_2_row = () =>
        {
            comboboxCells[2, 4].GetValue<string>().ShouldEqual("Option");
            comboboxCells[2, 5].GetValue<string>().ShouldEqual("Опция");
        };

        It should_export_translation_on__Translations_cascading__sheet_in_2_row = () =>
        {
            cascadingCells[2, 4].GetValue<string>().ShouldEqual("Cascading Option");
            cascadingCells[2, 5].GetValue<string>().ShouldEqual("Каскадная Опция");
        };

        static TranslationsService service;
        static ExcelRange comboboxCells;
        static ExcelRange cascadingCells;
        static readonly Guid translationId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid comboboxId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static readonly Guid parentId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static readonly Guid cascadingQustionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
    }
}