using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Moq;
using OfficeOpenXml;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.Questionnaire.Translations;

using TranslationInstance = WB.Core.BoundedContexts.Designer.Translations.TranslationInstance;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.TranslationServiceTests
{
    internal class when_questionnaire_contains_fixed_roster : TranslationsServiceTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
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
            BecauseOf();
        }

        private void BecauseOf() 
        {
            excelFile = service.GetAsExcelFile(questionnaireId, translationId);
            workbook = new ExcelPackage(new MemoryStream(excelFile.ContentAsExcelFile)).Workbook;
            cells = workbook.Worksheets[1].Cells;
        }

        [NUnit.Framework.Test] public void should_output_roster_title_translation () 
        {
            var questionTitleRow = 3;
            ((TranslationType)Enum.Parse(typeof(TranslationType), cells[questionTitleRow, translationTypeColumn].Text)).Should().Be(TranslationType.Title);
            cells[questionTitleRow, translationIndexColumn].Value?.ToString().Should().BeNull();
            cells[questionTitleRow, questionnaireEntityIdColumn].Value?.ToString().Should().Be(rosterId.FormatGuid());
            cells[questionTitleRow, originalTextColumn].Value?.ToString().Should().Be("non translated title");
            cells[questionTitleRow, translactionColumn].Value?.ToString().Should().BeNull();
        }

        [NUnit.Framework.Test] public void should_output_roster_fixed_option_title_translation () 
        {
            var questionTitleRow = 4;
            ((TranslationType)Enum.Parse(typeof(TranslationType), cells[questionTitleRow, translationTypeColumn].Text)).Should().Be(TranslationType.FixedRosterTitle);
            cells[questionTitleRow, translationIndexColumn].Value?.ToString().Should().Be("42");
            cells[questionTitleRow, questionnaireEntityIdColumn].Value?.ToString().Should().Be(rosterId.FormatGuid());
            cells[questionTitleRow, originalTextColumn].Value?.ToString().Should().Be("invariant option title");
            cells[questionTitleRow, translactionColumn].Value?.ToString().Should().Be("fixed roster item 1");
        }


        static Guid rosterId;
        static TranslationsService service;
        static Guid questionnaireId;
        static Guid translationId = Guid.Parse("ABBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        static TranslationFile excelFile;
        static ExcelWorkbook workbook;
        static ExcelRange cells;
    }
}