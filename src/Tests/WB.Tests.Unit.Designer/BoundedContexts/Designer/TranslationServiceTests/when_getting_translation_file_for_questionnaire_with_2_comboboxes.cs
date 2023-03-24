using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using ClosedXML.Graphics;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using SixLabors.Fonts;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.Questionnaire.Translations;


namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.TranslationServiceTests
{
    internal class when_getting_translation_file_for_questionnaire_with_2_comboboxes_with_same_variable_names_in_first_30_chars : TranslationsServiceTestsContext
    {
        [NUnit.Framework.OneTimeSetUp]
        public void context()
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
                Create.SingleQuestion(id: comboboxId, variable:$"{questionName}1", isFilteredCombobox: true, options: new List<Answer> {Create.Option("1", "Option")}),
                Create.SingleQuestion(id: cascadingQustionId, variable:$"{questionName}2", cascadeFromQuestionId: comboboxId, options: new List<Answer> {Create.Option("1", "Cascading Option", "1")})
            });

            var translationsStorage = Create.InMemoryDbContext();
            translationsStorage.TranslationInstances.AddRange(storedTranslations);
            translationsStorage.SaveChanges();

            var questionnaires = new Mock<IQuestionnaireViewFactory>();
            questionnaires.SetReturnsDefault(Create.QuestionnaireView(questionnaire));

            service = Create.TranslationsService(translationsStorage, questionnaires.Object);
            BecauseOf();
        }

        private void BecauseOf() => translationFile = service.GetAsExcelFile(new QuestionnaireRevision(questionnaireId), translationId);


        [NUnit.Framework.Test]
        public void should_exported_excel_file_has_3_specified_worksheets()
        {
            //non windows fonts
            var firstFont = SystemFonts.Collection.Families.First();
            var loadOptions = new LoadOptions { GraphicEngine = new DefaultGraphicEngine(firstFont.Name) };
            
            using var excelWorkbook = new XLWorkbook(new MemoryStream(translationFile.ContentAsExcelFile), loadOptions);
            var worksheetNames = excelWorkbook.Worksheets.Select(x => x.Name).ToList();

            worksheetNames.Should().BeEquivalentTo("Translations", "@@_singlequestionwithdiffinlast", "@@_singlequestionwithdiffinl_03");
        }

        TranslationFile translationFile;
        TranslationsService service;
        readonly Guid translationId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        readonly Guid comboboxId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        readonly Guid cascadingQustionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        readonly string questionName = "singlequestionwithdiffinlastcha";
    }
}
