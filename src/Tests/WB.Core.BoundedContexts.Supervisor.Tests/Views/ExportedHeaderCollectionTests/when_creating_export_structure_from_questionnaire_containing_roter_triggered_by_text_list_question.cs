using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Supervisor.Views.DataExport;

namespace WB.Core.BoundedContexts.Supervisor.Tests.Views.ExportedHeaderCollectionTests
{
    internal class when_creating_export_structure_from_questionnaire_containing_roter_triggered_by_text_list_question : QuestionnaireExportStructureTestsContext
    {
        private Establish context = () =>
        {
            rosterSizeQuestionId = Guid.Parse("AAF000AAA111EE2DD2EE111AAA000FFF");
            var rosterGroupId = Guid.Parse("00F000AAA111EE2DD2EE111AAA000FFF");
            questionInsideRosterId = Guid.Parse("CCF000AAA111EE2DD2EE111AAA000FFF");

            questionnaireDocument = CreateQuestionnaireDocumentWithOneChapter(
                new TextListQuestion("text list roster size question")
                {
                    PublicKey = rosterSizeQuestionId,
                    QuestionType = QuestionType.TextList,
                    MaxAnswerCount = maxAnswerCount
                },
                new Group("roster group")
                {
                    PublicKey = rosterGroupId,
                    IsRoster = true,
                    RosterSizeSource = RosterSizeSourceType.Question,
                    RosterSizeQuestionId = rosterSizeQuestionId,
                    Children = new List<IComposite>
                    {
                        new NumericQuestion() { PublicKey = questionInsideRosterId, QuestionType = QuestionType.Numeric }
                    }
                });
        };

        Because of = () =>
            questionnaireExportStructure = CreateQuestionnaireExportStructure(questionnaireDocument);

        It should_create_header_with_1_column = () =>
            questionnaireExportStructure.HeaderToLevelMap[rosterSizeQuestionId].HeaderItems[questionInsideRosterId].ColumnNames.Length.ShouldEqual(1);

        It should_create_header_with_5_columns_at_first_level = () =>
          questionnaireExportStructure.HeaderToLevelMap[questionnaireDocument.PublicKey].HeaderItems[rosterSizeQuestionId].ColumnNames.Length.ShouldEqual(5);

        private static QuestionnaireExportStructure questionnaireExportStructure;
        private static Guid questionInsideRosterId;
        private static QuestionnaireDocument questionnaireDocument;
        private static Guid rosterSizeQuestionId;
        private static int maxAnswerCount=5;
    }
}
