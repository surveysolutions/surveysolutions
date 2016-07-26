using System;
using System.Collections.Generic;
using System.IO;
using Aspose.Cells;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Moq;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.Questionnaire.Translations;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.TranslationServiceTests
{
    internal class when_getting_translation_file_for_one_question_with_non_printable_char : TranslationsServiceTestsContext
    {
        Establish context = () =>
        {
            char non_printable = (char) 1;

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

            var translationsStorage = new TestPlainStorage<TranslationInstance>();
            storedTranslations.ForEach(x => translationsStorage.Store(x, x));

            var questionnaires = new Mock<IReadSideKeyValueStorage<QuestionnaireDocument>>();
            questionnaires.SetReturnsDefault(questionnaire);

            service = Create.TranslationsService(translationsStorage, questionnaires.Object);
        };

        Because of = () =>
        {
            var excelFile = service.GetAsExcelFile(questionnaireId, translationId);
            cells = new Workbook(new MemoryStream(excelFile.ContentAsExcelFile)).Worksheets[0].Cells;
        };

        It should_remove_non_printable_chars_in_translation_file = () =>
        {
            cells[2, originalTextColumn].StringValue.ShouldEqual("В скобках символ без графического отобажения ()");
            cells[2, translactionColumn].StringValue.ShouldEqual("Here is non-printable char ()");
        };

        static TranslationsService service;
        static Cells cells;
        static readonly Guid translationId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid questionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
    }
}