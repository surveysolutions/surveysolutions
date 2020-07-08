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
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.Questionnaire.Translations;


namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.TranslationServiceTests
{
    internal class when_getting_translation_file_for_one_question_with_non_printable_char : TranslationsServiceTestsContext
    {
        [NUnit.Framework.Test]
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

            var questionnaires = new Mock<IPlainKeyValueStorage<QuestionnaireDocument>>();
            questionnaires.SetReturnsDefault(questionnaire);

            service = Create.TranslationsService(translationsStorage, questionnaires.Object);
            BecauseOf();

            
            cells.Cell(4, originalTextColumn).GetString().Should().Be("В скобках символ без графического отобажения ()");
            cells.Cell(4, translactionColumn).GetString().Should().Be("Here is non-printable char ()");
        }

        private void BecauseOf()
        {
            var excelFile = service.GetAsExcelFile(questionnaireId, translationId);
            cells = new XLWorkbook(new MemoryStream(excelFile.ContentAsExcelFile)).Worksheets.First();
        }

        static TranslationsService service;
        static IXLWorksheet cells;
        static readonly Guid translationId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid questionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
    }
}
