using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Tests.Unit.SharedKernels.DataCollection.Factories.QuestionnaireRosterStructureTests
{
    internal class when_getting_roster_scope_descriptios_for_questionnarie_which_has_multyoption_roster_group : QuestionnaireRosterStructureTestContext
    {
        Establish context = () =>
        {
            multyOptionRosterGroupId = new Guid("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            multyOptionQuestionId = new Guid("1111BBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            questionnarie = CreateQuestionnaireDocumentWithOneChapter(
                new MultyOptionsQuestion() { PublicKey = multyOptionQuestionId, QuestionType = QuestionType.MultyOption },
                new Group("Roster")
                {
                    PublicKey = multyOptionRosterGroupId,
                    IsRoster = true,
                    RosterSizeSource = RosterSizeSourceType.Question,
                    RosterSizeQuestionId = multyOptionQuestionId
                }
            );
            rosterStructureService = new RostrerStructureService();
        };

        Because of = () =>
            rosterScopes = rosterStructureService.GetRosterScopes(questionnarie);

        It should_contain_1_roster_scope = () =>
            rosterScopes.Count.ShouldEqual(1);

        It should_specify_multyoption_question_id_as_id_of_roster_scope = () =>
            rosterScopes.Single().Key.SequenceEqual(new[] { multyOptionQuestionId });

        It should_be_multyoption_scope_type_for_multyoption_roster_in_roster_scope = () =>
            rosterScopes.Single().Value.Type.ShouldEqual(RosterScopeType.MultyOption);

        private static QuestionnaireDocument questionnarie;
        private static IRostrerStructureService rosterStructureService;
        private static Dictionary<ValueVector<Guid>, RosterScopeDescription> rosterScopes;
        private static Guid multyOptionRosterGroupId;
        private static Guid multyOptionQuestionId;
    }
}
