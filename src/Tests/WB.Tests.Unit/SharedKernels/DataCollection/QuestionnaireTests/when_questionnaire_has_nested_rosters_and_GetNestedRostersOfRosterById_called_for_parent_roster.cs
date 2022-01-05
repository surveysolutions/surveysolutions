using System;
using System.Collections.Generic;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.GenericSubdomains.Portable;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.QuestionnaireTests
{
    internal class when_questionnaire_has_nested_rosters_and_GetNestedRostersOfRosterById_called_for_parent_roster : QuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            rosterGroupId = new Guid("EBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            questionnaireDocument = CreateQuestionnaireDocumentWithOneChapter(new IComposite[]
            {
                new NumericQuestion() { PublicKey = rosterSizeQuestionId, IsInteger = true},
                new Group()
                {
                    PublicKey = rosterGroupId,
                    IsRoster = true,
                    RosterSizeQuestionId = rosterSizeQuestionId,
                    Children = new List<IComposite>()
                        {
                            new Group("nested roster")
                            {
                                PublicKey = nestedRosterId,
                                RosterSizeSource = RosterSizeSourceType.FixedTitles,
                                IsRoster = true
                            }
                        }.ToReadOnlyCollection()
                }
            });
            BecauseOf();
        }

        public void BecauseOf() =>
            nestedRosters = Create.Entity.PlainQuestionnaire(questionnaireDocument, 1).GetNestedRostersOfGroupById(rosterGroupId);

        [NUnit.Framework.Test] public void should_rosterGroups_not_be_empty () =>
            nestedRosters.Should().NotBeEmpty();

        [NUnit.Framework.Test] public void should_rosterGroups_have_only_1_roster_group () =>
            nestedRosters.Should().BeEquivalentTo(nestedRosterId);

        private static IEnumerable<Guid> nestedRosters;
        private static QuestionnaireDocument questionnaireDocument;
        private static Guid rosterSizeQuestionId = new Guid("ABBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static Guid nestedRosterId = new Guid("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static Guid rosterGroupId;
    }
}
