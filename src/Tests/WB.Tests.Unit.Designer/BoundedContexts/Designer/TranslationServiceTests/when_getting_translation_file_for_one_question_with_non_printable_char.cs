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
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.TranslationServiceTests
{
    internal class when_getting_translation_file_for_one_question_with_non_printable_char : TranslationsServiceTestsContext
    {
        [Test]
        public void should_remove_non_printable_chars_in_translation_file()
        {
            char non_printable = (char)1;

            var storedTranslations = new List<TranslationInstance>
            {
                Create.TranslationInstance(type: TranslationType.Title,
                    translation: $"Here is non-printable char ({non_printable})",
                    translationId: translationId,
                    questionnaireId: questionnaireId,
                    questionnaireEntityId: questionId),
            };

            QuestionnaireDocument questionnaire = Create.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Question(questionId: questionId, title: $"В скобках символ без графического отобажения ({non_printable})")
            });

            var translationsStorage = Create.InMemoryDbContext();
            translationsStorage.AddRange(storedTranslations);
            translationsStorage.SaveChanges();

            var questionnaires = new Mock<IQuestionnaireViewFactory>();
            questionnaires.SetReturnsDefault(Create.QuestionnaireView(questionnaire));

            service = Create.TranslationsService(translationsStorage, questionnaires.Object);
            BecauseOf();


            cells.Cell(4, originalTextColumn).GetString().Should().Be("В скобках символ без графического отобажения ()");
            cells.Cell(4, translactionColumn).GetString().Should().Be("Here is non-printable char ()");
        }

        private void BecauseOf()
        {
            var excelFile = service.GetAsExcelFile(new QuestionnaireRevision(questionnaireId), translationId);
            workbook = new XLWorkbook(new MemoryStream(excelFile.ContentAsExcelFile));
            cells = workbook.Worksheets.First();
        }

        [TearDown]
        public void TearDown()
        {
            workbook?.Dispose();
        }

        TranslationsService service;
        IXLWorksheet cells;
        private readonly Guid translationId = Id.gD;
        readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        readonly Guid questionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private XLWorkbook workbook;
    }
}
