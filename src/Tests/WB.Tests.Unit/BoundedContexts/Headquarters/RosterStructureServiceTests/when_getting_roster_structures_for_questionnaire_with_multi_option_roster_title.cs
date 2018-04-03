using System;
using System.Collections.Generic;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.RosterStructureServiceTests
{
    internal class when_getting_roster_structures_for_questionnaire_with_multi_option_roster_title : RosterStructureServiceTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var rosterTitleQuestion = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var triggerId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            Guid rosterId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.NumericIntegerQuestion(id: triggerId),
                Create.Entity.Roster(rosterId: rosterId,
                    rosterSizeSourceType: RosterSizeSourceType.Question,
                    rosterSizeQuestionId: triggerId,
                    rosterTitleQuestionId: rosterTitleQuestion,
                    children: new IComposite[]
                    {
                        Create.Entity.MultyOptionsQuestion(id: rosterTitleQuestion,
                            options: new List<Answer>
                            {
                                Create.Entity.Answer("one", 1)
                            })
                    }));
            BecauseOf();
        }

        public void BecauseOf() => rosterStructures = GetService().GetRosterScopes(questionnaire);

        [NUnit.Framework.Test] public void should_build_some_roster_scopes () => rosterStructures.Count.Should().Be(1);

        static Dictionary<ValueVector<Guid>, RosterScopeDescription> rosterStructures;
        static QuestionnaireDocument questionnaire;
    }
}
