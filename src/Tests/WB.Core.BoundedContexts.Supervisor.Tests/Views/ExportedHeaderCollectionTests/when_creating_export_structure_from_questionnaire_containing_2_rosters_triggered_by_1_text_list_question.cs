using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Supervisor.Views.DataExport;

namespace WB.Core.BoundedContexts.Supervisor.Tests.Views.ExportedHeaderCollectionTests
{
    internal class when_creating_export_structure_from_questionnaire_containing_2_rosters_triggered_by_1_text_list_question : QuestionnaireExportStructureTestsContext
    {
        private Establish context = () =>
        {
            questionnaireDocument = CreateQuestionnaireDocumentWithOneChapter(
                new TextListQuestion("text list roster size question")
                {
                    PublicKey = rosterSizeQuestionId,
                    QuestionType = QuestionType.TextList,
                    MaxAnswerCount = maxAnswerCount
                },
                new Group("roster group 1")
                {
                    PublicKey = rosterGroup1Id,
                    IsRoster = true,
                    RosterSizeSource = RosterSizeSourceType.Question,
                    RosterSizeQuestionId = rosterSizeQuestionId,
                    Children = new List<IComposite>
                    {
                        new NumericQuestion() { PublicKey = questionInsideRoster1Id, QuestionType = QuestionType.Numeric }
                    }
                },
                new Group("roster group 2")
                {
                    PublicKey = rosterGroup2Id,
                    IsRoster = true,
                    RosterSizeSource = RosterSizeSourceType.Question,
                    RosterSizeQuestionId = rosterSizeQuestionId,
                    Children = new List<IComposite>
                    {
                        new NumericQuestion() { PublicKey = questionInsideRoster2Id, QuestionType = QuestionType.Numeric }
                    }
                });
        };

        Because of = () =>
            questionnaireExportStructure = CreateQuestionnaireExportStructure(questionnaireDocument);

        It should_create_header_with_1_column = () =>
            questionnaireExportStructure.HeaderToLevelMap[rosterSizeQuestionId].HeaderItems[questionInsideRoster1Id].ColumnNames.Length.ShouldEqual(1);

        It should_create_header_with_10_columns_at_first_level = () =>
          questionnaireExportStructure.HeaderToLevelMap[questionnaireDocument.PublicKey].HeaderItems[rosterSizeQuestionId].ColumnNames.Length.ShouldEqual(maxAnswerCount);

        private static QuestionnaireExportStructure questionnaireExportStructure;
        private static Guid questionInsideRoster1Id = Guid.Parse("CCF000AAA111EE2DD2EE111AAA000FFF");
        private static Guid questionInsideRoster2Id = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static QuestionnaireDocument questionnaireDocument;
        private static Guid rosterSizeQuestionId = Guid.Parse("AAF000AAA111EE2DD2EE111AAA000FFF");
        private static Guid rosterGroup1Id = Guid.Parse("00F000AAA111EE2DD2EE111AAA000FFF");
        private static Guid rosterGroup2Id = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static int maxAnswerCount = 5;
    }
}
