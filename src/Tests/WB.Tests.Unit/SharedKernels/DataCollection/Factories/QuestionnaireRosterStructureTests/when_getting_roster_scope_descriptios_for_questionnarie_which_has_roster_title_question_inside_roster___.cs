using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Tests.Unit.SharedKernels.DataCollection.Factories.QuestionnaireRosterStructureTests
{
    internal class when_getting_roster_scope_descriptions_for_questionnarie_which_has_roster_title_question_inside_roster_group : QuestionnaireRosterStructureTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            rosterTitleQuestionId = new Guid("CBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            rosterSizeQuestionId = new Guid("EBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            rosterGroupId = new Guid("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            questionnarie = CreateQuestionnaireDocumentWithOneChapter(

                new NumericQuestion { PublicKey = rosterSizeQuestionId },

                new Group("Roster")
                {
                    PublicKey = rosterGroupId,
                    IsRoster = true,
                    RosterSizeQuestionId = rosterSizeQuestionId,
                    RosterTitleQuestionId = rosterTitleQuestionId,

                    Children = new List<IComposite>
                    {
                        new NumericQuestion { PublicKey = rosterTitleQuestionId },
                    }.ToReadOnlyCollection()
                }
            );
            rosterStructureService = new RosterStructureService();
            BecauseOf();
        }

        public void BecauseOf() =>
            rosterScopes = rosterStructureService.GetRosterScopes(questionnarie);

        [NUnit.Framework.Test] public void should_contain_1_roster_scope () =>
            rosterScopes.Count.Should().Be(1);

        [NUnit.Framework.Test] public void should_specify_roster_size_question_id_as_id_of_roster_scope () =>
            rosterScopes.Single().Key.Should().BeEquivalentTo(new[] { rosterSizeQuestionId });

        [NUnit.Framework.Test] public void should_specify_id_of_roster_title_question_as_roster_title_question_id_for_roster_id_in_roster_scope () =>
            rosterScopes.Single().Value
                .RosterIdToRosterTitleQuestionIdMap[rosterGroupId].QuestionId.Should().Be(rosterTitleQuestionId);

        private static QuestionnaireDocument questionnarie;
        private static IRosterStructureService rosterStructureService;
        private static Dictionary<ValueVector<Guid>, RosterScopeDescription> rosterScopes;
        private static Guid rosterTitleQuestionId;
        private static Guid rosterGroupId;
        private static Guid rosterSizeQuestionId;
    }
}
