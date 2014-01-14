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
using WB.Core.BoundedContexts.Supervisor.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Supervisor.Tests.Views.ExportedHeaderCollectionTests
{
    internal class when_creating_structure_from_questionnaire_containing_fixed_roster_and_multy_option_linked_question_to_header : QuestionnaireExportStructureTestsContext
    {
        Establish context = () =>
        {
            rosterGroupId = Guid.Parse("00F000AAA111EE2DD2EE111AAA000FFF");
            linkedQuestionId = Guid.Parse("BBF000AAA111EE2DD2EE111AAA000FFF");
            referencedQuestionId = Guid.Parse("CCF000AAA111EE2DD2EE111AAA000FFF");

            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                new Group("roster group")
                {
                    PublicKey = rosterGroupId,
                    IsRoster = true,
                    RosterSizeSource = RosterSizeSourceType.FixedTitles,
                    RosterFixedTitles = new string[] { "1", "2" },
                    Children = new List<IComposite>
                    {
                        new NumericQuestion() { PublicKey = referencedQuestionId, QuestionType = QuestionType.Numeric },
                        new MultyOptionsQuestion() { LinkedToQuestionId = referencedQuestionId, PublicKey = linkedQuestionId, QuestionType = QuestionType.MultyOption }
                    }
                });
        };

        Because of = () =>
            questionnaireExportStructure = CreateQuestionnaireExportStructure(questionnaire);

        It should_create_header_with_2_column = () =>
            questionnaireExportStructure.HeaderToLevelMap[rosterGroupId].HeaderItems[linkedQuestionId].ColumnNames.Length.ShouldEqual(2);

        private static QuestionnaireExportStructure questionnaireExportStructure;
        private static Guid linkedQuestionId;
        private static Guid referencedQuestionId;
        private static QuestionnaireDocument questionnaire;
        private static Guid rosterGroupId;
    }
}
