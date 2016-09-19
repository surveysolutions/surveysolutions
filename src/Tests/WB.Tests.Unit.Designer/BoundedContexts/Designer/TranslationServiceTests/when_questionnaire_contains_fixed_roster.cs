using System;
using System.Collections.Generic;
using System.IO;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Moq;
using SpreadsheetGear;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.Questionnaire.Translations;
using It = Machine.Specifications.It;
using TranslationInstance = WB.Core.BoundedContexts.Designer.Translations.TranslationInstance;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.TranslationServiceTests
{
    internal class when_questionnaire_contains_fixed_roster : TranslationsServiceTestsContext
    {
        Establish context = () =>
        {
            questionnaireId = Guid.Parse("11111111111111111111111111111111");
            rosterId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            var storedTranslations = new List<TranslationInstance>
            {
                Create.TranslationInstance(type: TranslationType.FixedRosterTitle,
                    translation: "fixed roster item 1",
                    translationId: translationId,
                    questionnaireId: questionnaireId,
                    questionnaireEntityId: rosterId,
                    translationIndex: "42")
            };

            QuestionnaireDocument questionnaire = Create.QuestionnaireDocumentWithOneChapter(
                children: new IComposite[]
                {
                    Create.FixedRoster(rosterId: rosterId,
                        title: "non translated title",
                        fixedRosterTitles: new[] {Create.FixedRosterTitle(42, "invariant option title")}
                       )
                });

            var translationsStorage = new TestPlainStorage<TranslationInstance>();
            foreach (var translationInstance in storedTranslations)
            {
                translationsStorage.Store(translationInstance, translationInstance);
            }

            var questionnaires = new Mock<IPlainKeyValueStorage<QuestionnaireDocument>>();
            questionnaires.SetReturnsDefault(questionnaire);

            service = Create.TranslationsService(translationsStorage, questionnaires.Object);
        };

        Because of = () =>
        {
            excelFile = service.GetAsExcelFile(questionnaireId, translationId);
            workbook = SpreadsheetGear.Factory.GetWorkbookSet().Workbooks.OpenFromMemory(excelFile.ContentAsExcelFile);
            cells = workbook.Worksheets[0].Cells;
        };

        It should_output_roster_title_translation = () =>
        {
            var questionTitleRow = 2;
            ((TranslationType)Enum.Parse(typeof(TranslationType), cells[questionTitleRow, translationTypeColumn].Text)).ShouldEqual(TranslationType.Title);
            cells[questionTitleRow, translationIndexColumn].Value?.ToString().ShouldBeNull();
            cells[questionTitleRow, questionnaireEntityIdColumn].Value?.ToString().ShouldEqual(rosterId.FormatGuid());
            cells[questionTitleRow, originalTextColumn].Value?.ToString().ShouldEqual("non translated title");
            cells[questionTitleRow, translactionColumn].Value?.ToString().ShouldBeNull();
        };

        It should_output_roster_fixed_option_title_translation = () =>
        {
            var questionTitleRow = 3;
            ((TranslationType)Enum.Parse(typeof(TranslationType), cells[questionTitleRow, translationTypeColumn].Text)).ShouldEqual(TranslationType.FixedRosterTitle);
            cells[questionTitleRow, translationIndexColumn].Value?.ToString().ShouldEqual("42");
            cells[questionTitleRow, questionnaireEntityIdColumn].Value?.ToString().ShouldEqual(rosterId.FormatGuid());
            cells[questionTitleRow, originalTextColumn].Value?.ToString().ShouldEqual("invariant option title");
            cells[questionTitleRow, translactionColumn].Value?.ToString().ShouldEqual("fixed roster item 1");
        };


        static Guid rosterId;
        static TranslationsService service;
        static Guid questionnaireId;
        static Guid translationId = Guid.Parse("ABBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        static TranslationFile excelFile;
        static IWorkbook workbook;
        static IRange cells;
    }
}