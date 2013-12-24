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
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.SharedKernels.DataCollection.Tests.Views.QuestionnaireRosterStructureTests
{
    internal class when_creating_roster_structure_for_questionnarie_which_has_fixed_roster_group : QuestionnaireRosterStructureTestContext
    {
        Establish context = () =>
        {
            fixedRosterGroupId = new Guid("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            questionnarie = CreateQuestionnaireDocumentWithOneChapter(
                new Group("Roster")
                {
                    PublicKey = fixedRosterGroupId,
                    IsRoster = true,
                    RosterSizeSource = RosterSizeSourceType.FixedTitles
                }
            );
        };

        Because of = () =>
            questionnaireRosterStructure = new QuestionnaireRosterStructure(questionnarie, 1);

        It should_contain_1_roster_scope = () =>
            questionnaireRosterStructure.RosterScopes.Count().ShouldEqual(1);

        It should_specify_fixed_roster_id_as_id_of_roster_scope = () =>
            questionnaireRosterStructure.RosterScopes.Single().Key.ShouldEqual(fixedRosterGroupId);

        It should_be_null_roster_title_question_for_fixed_roster_in_roster_scope = () =>
            questionnaireRosterStructure.RosterScopes.Single().Value
                .RosterIdToRosterTitleQuestionIdMap[fixedRosterGroupId].ShouldBeNull();

        private static QuestionnaireDocument questionnarie;
        private static QuestionnaireRosterStructure questionnaireRosterStructure;
        private static Guid fixedRosterGroupId;
    }
}
