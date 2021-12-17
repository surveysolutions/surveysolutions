using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;


namespace WB.Tests.Unit.SharedKernels.DataCollection.Factories.QuestionnaireRosterStructureTests
{
    internal class when_getting_roster_scope_descriptios_for_questionnarie_which_has_textlist_roster_group : QuestionnaireRosterStructureTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            textlistRosterGroupId = new Guid("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            textlistQuestionId = new Guid("1111BBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            questionnarie = CreateQuestionnaireDocumentWithOneChapter(
                new TextListQuestion() { PublicKey = textlistQuestionId },
                new Group("Roster")
                {
                    PublicKey = textlistRosterGroupId,
                    IsRoster = true,
                    RosterSizeSource = RosterSizeSourceType.Question,
                    RosterSizeQuestionId = textlistQuestionId
                }
            );
            
            rosterStructureService = new RosterStructureService();
            BecauseOf();
        }

        public void BecauseOf() =>
            rosterScopes = rosterStructureService.GetRosterScopes(questionnarie);

        [NUnit.Framework.Test] public void should_contain_1_roster_scope () =>
            rosterScopes.Count().Should().Be(1);

        [NUnit.Framework.Test] public void should_specify_textlist_question_id_as_id_of_roster_scope () =>
            rosterScopes.Single().Key.Should().BeEquivalentTo(new[] { textlistQuestionId });

        [NUnit.Framework.Test] public void should_be_textlist_scope_type_for_textlist_roster_in_roster_scope () =>
            rosterScopes.Single().Value.Type.Should().Be(RosterScopeType.TextList);

        private static QuestionnaireDocument questionnarie;
        private static IRosterStructureService rosterStructureService;
        private static Dictionary<ValueVector<Guid>, RosterScopeDescription> rosterScopes;
        private static Guid textlistRosterGroupId;
        private static Guid textlistQuestionId;
    }
}
