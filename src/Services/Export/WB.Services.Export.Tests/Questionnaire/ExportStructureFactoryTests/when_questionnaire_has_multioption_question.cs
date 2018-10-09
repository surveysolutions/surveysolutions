using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Questionnaire.Services;

namespace WB.Services.Export.Tests.Questionnaire.ExportStructureFactoryTests
{
    internal class when_questionnaire_has_multioption_question : ExportViewFactoryTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionId = Guid.Parse("d7127d06-5668-4fa3-b255-8a2a0aaaa020");
            questionnaire = Create.QuestionnaireDocumentWithOneChapter(
                Create.MultyOptionsQuestion(id: questionId, 
                variable:"mult",
                options: new List<Answer> {
                    Create.Answer("foo", 28), Create.Answer("bar", 42)
                }));

            QuestionnaireExportStructureFactory = CreateExportViewFactory();
            BecauseOf();
        }

        public void BecauseOf() => questionnaaireExportStructure = QuestionnaireExportStructureFactory.CreateQuestionnaireExportStructure(questionnaire);

        [NUnit.Framework.Test] public void should_fill_multioption_question_header_title () 
        {
            HeaderStructureForLevel headerStructureForLevel = questionnaaireExportStructure.HeaderToLevelMap[new ValueVector<Guid>()];
            ExportedQuestionHeaderItem exportedQuestionHeaderItem = headerStructureForLevel.HeaderItems[questionId] as ExportedQuestionHeaderItem;

            exportedQuestionHeaderItem.ColumnHeaders.Count.Should().Be(2);
            exportedQuestionHeaderItem.ColumnHeaders.Select(x=> x.Name).SequenceEqual(new[] { "mult__28", "mult__42" }).Should().BeTrue();
            exportedQuestionHeaderItem.ColumnValues.Length.Should().Be(2);
            exportedQuestionHeaderItem.ColumnValues.SequenceEqual(new[] {28, 42}).Should().BeTrue();
        }

        static QuestionnaireExportStructureFactory QuestionnaireExportStructureFactory;
        static QuestionnaireExportStructure questionnaaireExportStructure;
        static QuestionnaireDocument questionnaire;
        static Guid questionId;
    }
}
