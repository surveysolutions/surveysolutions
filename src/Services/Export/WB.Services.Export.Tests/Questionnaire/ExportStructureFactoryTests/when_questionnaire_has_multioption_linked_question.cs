using System;
using System.Linq;
using FluentAssertions;
using Moq;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.Tests.Questionnaire.ExportStructureFactoryTests
{
    internal class when_questionnaire_has_multioption_linked_question : ExportViewFactoryTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            multyOptionLinkedQuestionId = Guid.Parse("d7127d06-5668-4fa3-b255-8a2a0aaaa020");
            linkedSourceQuestionId = Guid.NewGuid();

            questionnaire = Create.QuestionnaireDocumentWithOneChapter(
                Create.Roster(rosterId: Guid.NewGuid(), 
                    variable: "row", 
                    fixedTitles: new FixedRosterTitle[] {new FixedRosterTitle(1, "1"), new FixedRosterTitle(2, "2")},
                    children: new[]
                    {
                        Create.TextQuestion(id: linkedSourceQuestionId, variable: "varTxt")
                    }),
                Create.MultyOptionsQuestion(id: multyOptionLinkedQuestionId,
                    variable: "mult",
                    linkedToQuestionId: linkedSourceQuestionId));

            QuestionnaireExportStructureFactory = CreateExportViewFactory();
            BecauseOf();
        }

        public void BecauseOf() => questionnaaireExportStructure = QuestionnaireExportStructureFactory.CreateQuestionnaireExportStructure(questionnaire);

        [NUnit.Framework.Test] public void should_fill_multioption_question_header_title () 
        {
            HeaderStructureForLevel headerStructureForLevel = questionnaaireExportStructure.HeaderToLevelMap[new ValueVector<Guid>()];
            ExportedQuestionHeaderItem exportedQuestionHeaderItem = headerStructureForLevel.HeaderItems[multyOptionLinkedQuestionId] as ExportedQuestionHeaderItem;

            exportedQuestionHeaderItem.ColumnHeaders.Count.Should().Be(2);
            exportedQuestionHeaderItem.ColumnHeaders.Select(x=>x.Name).SequenceEqual(new[] { "mult__0", "mult__1" }).Should().BeTrue();
            exportedQuestionHeaderItem.QuestionSubType.Should().Be(QuestionSubtype.MultyOption_Linked);
            exportedQuestionHeaderItem.QuestionType.Should().Be(QuestionType.MultyOption);
        }

        static QuestionnaireExportStructureFactory QuestionnaireExportStructureFactory;
        static QuestionnaireExportStructure questionnaaireExportStructure;
        static QuestionnaireDocument questionnaire;
        static Guid multyOptionLinkedQuestionId;
        static Guid linkedSourceQuestionId;
    }
}
