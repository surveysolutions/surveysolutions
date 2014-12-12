using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Factories;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Tests.Unit.SharedKernels.DataCollection.Factories.QuestionnaireRosterStructureFactoryTests
{
    internal class when_creating_roster_structure_factory_for_questionnarie_which_has_fixed_roster_group : QuestionnaireRosterStructureFactoryTestContext
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
            questionnaireRosterStructureFactory = CreateQuestionnaireRosterStructureFactory();
        };

        Because of = () =>
            questionnaireRosterStructure = questionnaireRosterStructureFactory.CreateQuestionnaireRosterStructure(questionnarie, 1);

        It should_contain_1_roster_scope = () =>
            questionnaireRosterStructure.RosterScopes.Count().ShouldEqual(1);

        It should_specify_fixed_roster_id_as_id_of_roster_scope = () =>
            questionnaireRosterStructure.RosterScopes.Single().Key.SequenceEqual(new[] { fixedRosterGroupId });

        It should_be_null_roster_title_question_for_fixed_roster_in_roster_scope = () =>
            questionnaireRosterStructure.RosterScopes.Single().Value
                .RosterIdToRosterTitleQuestionIdMap[fixedRosterGroupId].ShouldBeNull();

        It should_be_fixed_scope_type_for_fixed_roster_in_roster_scope = () =>
            questionnaireRosterStructure.RosterScopes.Single().Value.ScopeType.ShouldEqual(RosterScopeType.Fixed);

        private static QuestionnaireDocument questionnarie;
        private static QuestionnaireRosterStructureFactory questionnaireRosterStructureFactory;
        private static QuestionnaireRosterStructure questionnaireRosterStructure;
        private static Guid fixedRosterGroupId;
    }
}
