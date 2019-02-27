using System;
using System.Linq;
using NUnit.Framework;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Tests.Questionnaire.ExportStructureFactoryTests;

namespace WB.Services.Export.Tests.Questionnaire
{
    [TestOf(typeof(QuestionnaireExportStructureFactory))]
    internal class QuestionnaireExportStructureFactoryTests : ExportViewFactoryTestsContext
    {
        [Test]
        public void should_fill_multioption_question_header_title()
        {
            // arrange
            var multyOptionLinkedQuestionId = Guid.Parse("d7127d06-5668-4fa3-b255-8a2a0aaaa020");
            var linkedSourceQuestionId = Guid.NewGuid();

            var questionnaire = Create.QuestionnaireDocumentWithOneChapter(
                Create.Roster(rosterId: Guid.NewGuid(),
                    variable: "row",
                    fixedTitles: new FixedRosterTitle[] { new FixedRosterTitle(1, "1"), new FixedRosterTitle(2, "2") },
                    children: new[]
                    {
                        Create.TextQuestion(id: linkedSourceQuestionId, variable: "varTxt")
                    }),
                Create.MultyOptionsQuestion(id: multyOptionLinkedQuestionId,
                    variable: "mult",
                    linkedToQuestionId: linkedSourceQuestionId));

            var QuestionnaireExportStructureFactory = CreateExportViewFactory();


            // act
            var questionnaaireExportStructure = QuestionnaireExportStructureFactory.CreateQuestionnaireExportStructure(questionnaire);

            // assert
            HeaderStructureForLevel headerStructureForLevel = questionnaaireExportStructure.HeaderToLevelMap[new ValueVector<Guid>()];
            ExportedQuestionHeaderItem exportedQuestionHeaderItem = headerStructureForLevel.HeaderItems[multyOptionLinkedQuestionId] as ExportedQuestionHeaderItem;

            Assert.That(exportedQuestionHeaderItem.ColumnHeaders.Count, Is.EqualTo(2));
            Assert.That(exportedQuestionHeaderItem.ColumnHeaders.Select(x => x.Name).ToArray(), Is.EquivalentTo(new[] { "mult__0", "mult__1" }));

            Assert.That(exportedQuestionHeaderItem.ColumnHeaders[0].ExportType, Is.EqualTo(ExportValueType.String));
            Assert.That(exportedQuestionHeaderItem.ColumnHeaders[1].ExportType, Is.EqualTo(ExportValueType.String));

            Assert.That(exportedQuestionHeaderItem.QuestionSubType, Is.EqualTo(QuestionSubtype.MultyOption_Linked));
            Assert.That(exportedQuestionHeaderItem.QuestionType, Is.EqualTo(QuestionType.MultyOption));
        }
    }
}
