using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Tests.Unit.SharedKernels.DataCollection.Factories.QuestionnaireRosterStructureTests
{
    internal class when_getting_roster_scope_descriptios_for_questionnaire_which_has_fixed_roster_group : QuestionnaireRosterStructureTestContext
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
            rosterStructureService = new RostrerStructureService();
        };

        Because of = () =>
            rosterScopes = rosterStructureService.GetRosterScopes(questionnarie);

        It should_contain_1_roster_scope = () =>
            rosterScopes.Count.ShouldEqual(1);

        It should_specify_fixed_roster_id_as_id_of_roster_scope = () =>
            rosterScopes.Single().Key.SequenceEqual(new[] { fixedRosterGroupId });

        It should_be_null_roster_title_question_for_fixed_roster_in_roster_scope = () =>
            rosterScopes.Single().Value
                .RosterIdToRosterTitleQuestionIdMap[fixedRosterGroupId].ShouldBeNull();

        It should_be_fixed_scope_type_for_fixed_roster_in_roster_scope = () =>
            rosterScopes.Single().Value.Type.ShouldEqual(RosterScopeType.Fixed);

        private static Guid questionnaireId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBA");
        private static IRostrerStructureService rosterStructureService;
        private static QuestionnaireDocument questionnarie;
        private static Dictionary<ValueVector<Guid>, RosterScopeDescription> rosterScopes;
        private static Guid fixedRosterGroupId;
    }
}
