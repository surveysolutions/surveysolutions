using System;
using System.Collections.Generic;
using System.IO;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Moq;
using OfficeOpenXml;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.GenericSubdomains.Portable;
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
                    culture: cultureId.FormatGuid(),
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

            var questionnaires = new Mock<IReadSideKeyValueStorage<QuestionnaireDocument>>();
            questionnaires.SetReturnsDefault(questionnaire);

            service = CreateTranslationsService(translationsStorage, questionnaires.Object);
        };

        Because of = () =>
        {
            excelFile = service.GetAsExcelFile(questionnaireId, cultureId);
            var memory = new MemoryStream(excelFile.ContentAsExcelFile);
            package = new ExcelPackage(memory);
            cells = package.Workbook.Worksheets[1].Cells;
        };

        It should_output_roster_title_translation = () =>
        {
            var questionTitleRow = 3;
            ((TranslationType)cells[questionTitleRow, translationTypeColumn].GetValue<int>()).ShouldEqual(TranslationType.Title);
            cells[questionTitleRow, translationIndexColumn].GetValue<string>().ShouldBeNull();
            cells[questionTitleRow, questionnaireEntityIdColumn].GetValue<string>().ShouldEqual(rosterId.ToString());
            cells[questionTitleRow, originalTextColumn].GetValue<string>().ShouldEqual("non translated title");
            cells[questionTitleRow, translactionColumn].GetValue<string>().ShouldBeNull();
        };

        It should_output_roster_fixed_option_title_translation = () =>
        {
            var questionTitleRow = 4;
            ((TranslationType)cells[questionTitleRow, translationTypeColumn].GetValue<int>()).ShouldEqual(TranslationType.FixedRosterTitle);
            cells[questionTitleRow, translationIndexColumn].GetValue<string>().ShouldEqual("42");
            cells[questionTitleRow, questionnaireEntityIdColumn].GetValue<string>().ShouldEqual(rosterId.ToString());
            cells[questionTitleRow, originalTextColumn].GetValue<string>().ShouldEqual("invariant option title");
            cells[questionTitleRow, translactionColumn].GetValue<string>().ShouldEqual("fixed roster item 1");
        };


        Cleanup things = () => package?.Dispose();

        static Guid rosterId;
        static TranslationsService service;
        static Guid questionnaireId;
        static Guid cultureId = Guid.Parse("ABBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        static TranslationFile excelFile;
        static ExcelPackage package;
        static ExcelRange cells;
    }
}