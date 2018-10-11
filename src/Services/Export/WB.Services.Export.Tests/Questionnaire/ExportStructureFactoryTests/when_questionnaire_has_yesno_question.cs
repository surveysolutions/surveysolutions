using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Questionnaire.Services;

namespace WB.Services.Export.Tests.Questionnaire.ExportStructureFactoryTests
{
    internal class when_questionnaire_has_yesno_question : ExportViewFactoryTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionId = Guid.Parse("d7127d06-5668-4fa3-b255-8a2a0aaaa020");
            questionnaire = Create.QuestionnaireDocumentWithOneChapter(
                Create.MultyOptionsQuestion(id: questionId,
                    variable: "mult",
                    yesNoView: true,
                    options: new List<Answer> {
                        Create.Answer("foo", 28), Create.Answer("bar", 42)
                    }));

            var questionnaireMockStorage = new Mock<IQuestionnaireStorage>();
            QuestionnaireExportStructureFactory = CreateExportViewFactory(questionnaireMockStorage.Object);
            BecauseOf();
        }

        public void BecauseOf() => questionnaaireExportStructure = QuestionnaireExportStructureFactory.CreateQuestionnaireExportStructure(questionnaire);

        [NUnit.Framework.Test] public void should_fill_yesno_question_subtype () 
        {
            HeaderStructureForLevel headerStructureForLevel = questionnaaireExportStructure.HeaderToLevelMap[new ValueVector<Guid>()];
            ExportedQuestionHeaderItem exportedQuestionHeaderItem = headerStructureForLevel.HeaderItems[questionId] as ExportedQuestionHeaderItem;

            exportedQuestionHeaderItem.ColumnValues.Length.Should().Be(2);
            exportedQuestionHeaderItem.QuestionSubType.Should().Be(QuestionSubtype.MultyOption_YesNo);
        }

        static QuestionnaireExportStructureFactory QuestionnaireExportStructureFactory;
        static QuestionnaireExportStructure questionnaaireExportStructure;
        static QuestionnaireDocument questionnaire;
        static Guid questionId;
    }
}
