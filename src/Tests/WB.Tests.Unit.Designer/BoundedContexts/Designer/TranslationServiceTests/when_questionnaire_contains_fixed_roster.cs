using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Tests.Abc;
using TranslationInstance = WB.Core.BoundedContexts.Designer.Translations.TranslationInstance;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.TranslationServiceTests
{
    internal class when_questionnaire_contains_fixed_roster : TranslationsServiceTestsContext
    {
        [SetUp]
        public void context()
        {
            questionnaireId = Id.g1;
            rosterId = Id.gB;

            var storedTranslations = new List<TranslationInstance>
            {
                Create.TranslationInstance(type: TranslationType.FixedRosterTitle,
                    translation: "fixed roster item 1",
                    translationId: translationId,
                    questionnaireId: questionnaireId,
                    questionnaireEntityId: rosterId,
                    translationIndex: "42")
            };

            QuestionnaireDocument questionnaire = Create.OldQuestionnaireDocumentWithOneChapter(
                children: new IComposite[]
                {
                    Create.FixedRoster(rosterId: rosterId,
                        title: "non translated title",
                        fixedRosterTitles: new[] {Create.FixedRosterTitle(42, "invariant option title")}
                       )
                },questionnaireId:questionnaireId);

            var translationsStorage = Create.InMemoryDbContext();
            translationsStorage.TranslationInstances.AddRange(storedTranslations);
            translationsStorage.SaveChanges();

            var questionnaires = new Mock<IQuestionnaireViewFactory>();
            questionnaires.SetReturnsDefault(Create.QuestionnaireView(questionnaire));

            service = Create.TranslationsService(translationsStorage, questionnaires.Object);
            BecauseOf();
        }

        private void BecauseOf()
        {
            excelFile = service.GetAsExcelFile(new QuestionnaireRevision(questionnaireId), translationId);
            workbook = new XLWorkbook(new MemoryStream(excelFile.ContentAsExcelFile));
            cells = workbook.Worksheets.First();
        }

        [TearDown]
        public void TearDown()
        {
            workbook?.Dispose();
        }

        [NUnit.Framework.Test]
        public void should_output_roster_title_translation()
        {
            var questionTitleRow = 4;
            ((TranslationType)Enum.Parse(typeof(TranslationType), cells.Cell(questionTitleRow, translationTypeColumn).GetString())).Should().Be(TranslationType.Title);
            cells.Cell(questionTitleRow, translationIndexColumn).Value?.ToString().Should().BeEmpty();
            cells.Cell(questionTitleRow, questionnaireEntityIdColumn).Value?.ToString().Should().Be(rosterId.FormatGuid());
            cells.Cell(questionTitleRow, originalTextColumn).Value?.ToString().Should().Be("non translated title");
            cells.Cell(questionTitleRow, translactionColumn).Value?.ToString().Should().BeEmpty();
        }

        [NUnit.Framework.Test]
        public void should_output_roster_fixed_option_title_translation()
        {
            var questionTitleRow = 5;
            ((TranslationType)Enum.Parse(typeof(TranslationType), cells.Cell(questionTitleRow, translationTypeColumn).GetString())).Should().Be(TranslationType.FixedRosterTitle);
            cells.Cell(questionTitleRow, translationIndexColumn).Value?.ToString().Should().Be("42");
            cells.Cell(questionTitleRow, questionnaireEntityIdColumn).Value?.ToString().Should().Be(rosterId.FormatGuid());
            cells.Cell(questionTitleRow, originalTextColumn).Value?.ToString().Should().Be("invariant option title");
            cells.Cell(questionTitleRow, translactionColumn).Value?.ToString().Should().Be("fixed roster item 1");
        }

        Guid rosterId;
        TranslationsService service;
        Guid questionnaireId;
        Guid translationId = Id.gC;
        TranslationFile excelFile;
        IXLWorkbook workbook;
        private IXLWorksheet cells;
    }
}
