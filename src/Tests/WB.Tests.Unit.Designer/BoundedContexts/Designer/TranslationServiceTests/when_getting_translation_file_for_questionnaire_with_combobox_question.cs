using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.Questionnaire.Translations;


namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.TranslationServiceTests
{
    internal class when_getting_translation_file_for_questionnaire_with_combobox_and_cascading_question : TranslationsServiceTestsContext
    {
        [SetUp]
        public void context()
        {
            var storedTranslations = new List<TranslationInstance>
            {
                Create.TranslationInstance(type: TranslationType.OptionTitle,
                    translation: "Каскадная Опция",
                    translationIndex: "1$1",
                    translationId: translationId,
                    questionnaireId: questionnaireId,
                    questionnaireEntityId: cascadingQustionId),
                Create.TranslationInstance(type: TranslationType.OptionTitle,
                    translation: "Опция",
                    translationIndex: "1$",
                    translationId: translationId,
                    questionnaireId: questionnaireId,
                    questionnaireEntityId: comboboxId),
            };

            QuestionnaireDocument questionnaire = Create.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.SingleQuestion(id: comboboxId, variable:"combobox", isFilteredCombobox: true, options: new List<Answer> {Create.Option("1", "Option")}),
                Create.SingleQuestion(id: cascadingQustionId, variable:"cascading", cascadeFromQuestionId: comboboxId, options: new List<Answer> {Create.Option("1", "Cascading Option", "1")})
            },
                questionnaireId:questionnaireId);

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
            var excelFile = service.GetAsExcelFile(new QuestionnaireRevision(questionnaireId), translationId);
            this.workbook = new XLWorkbook(new MemoryStream(excelFile.ContentAsExcelFile));
            comboboxCells = workbook.Worksheets.First(x => x.Name == "@@_combobox");
            cascadingCells = workbook.Worksheets.First(x => x.Name == "@@_cascading");
        }

        [TearDown]
        public void TearDown()
        {
            workbook?.Dispose();
        }

        [NUnit.Framework.Test]
        public void should_export_translation_on__Translations_combobox__sheet_in_2_row()
        {
            comboboxCells.Cell(2, TranslationsServiceTestsContext.originalTextColumn).GetValue<string>().Should().Be("Option");
            comboboxCells.Cell(2, TranslationsServiceTestsContext.translactionColumn).GetValue<string>().Should().Be("Опция");
        }

        [NUnit.Framework.Test]
        public void should_export_translation_on__Translations_cascading__sheet_in_2_row()
        {
            cascadingCells.Cell(2, TranslationsServiceTestsContext.originalTextColumn).GetValue<string>().Should().Be("Cascading Option");
            cascadingCells.Cell(2, TranslationsServiceTestsContext.translactionColumn).GetValue<string>().Should().Be("Каскадная Опция");
        }

        TranslationsService service;
        IXLWorksheet comboboxCells;
        IXLWorksheet cascadingCells;
        readonly Guid translationId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        readonly Guid comboboxId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        readonly Guid parentId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        readonly Guid cascadingQustionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private XLWorkbook workbook;
    }
}
