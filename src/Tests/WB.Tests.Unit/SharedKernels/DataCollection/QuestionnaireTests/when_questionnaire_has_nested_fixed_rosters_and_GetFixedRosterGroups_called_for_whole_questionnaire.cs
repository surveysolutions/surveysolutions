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
    internal class when_questionnaire_has_nested_fixed_rosters_and_GetFixedRosterGroups_called_for_whole_questionnaire : QuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            rosterGroupId = new Guid("EBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            questionnaireDocument = CreateQuestionnaireDocumentWithOneChapter(new IComposite[]
            {
                new NumericQuestion()
                {
                    PublicKey = rosterSizeQuestionId,
                    IsInteger = true
                },
                new Group()
                {
                    PublicKey = rosterGroupId,
                    IsRoster = true,
                    RosterSizeQuestionId = rosterSizeQuestionId,
                    Children =
                        new List<IComposite>()
                        {
                            new Group("nested roster")
                            {
                                PublicKey = nestedRosterId,
                                RosterSizeSource = RosterSizeSourceType.FixedTitles,
                                IsRoster = true
                            }
                        }.ToReadOnlyCollection()
                },
                new Group("fixed roster")
                {
                    PublicKey = fixedRosterId,
                    RosterSizeSource = RosterSizeSourceType.FixedTitles,
                    IsRoster = true
                }
            });
            BecauseOf();
        }

        public void BecauseOf() =>
            nestedRosters = Create.Entity.PlainQuestionnaire(questionnaireDocument, 1).GetFixedRosterGroups(null);

        [NUnit.Framework.Test] public void should_rosterGroups_not_be_empty () =>
            nestedRosters.Should().NotBeEmpty();

        [NUnit.Framework.Test] public void should_rosterGroups_have_only_1_roster_group () =>
            nestedRosters.Should().BeEquivalentTo(new []{fixedRosterId});

        private static IEnumerable<Guid> nestedRosters;
        private static QuestionnaireDocument questionnaireDocument;
        private static Guid rosterSizeQuestionId = new Guid("ABBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static Guid nestedRosterId = new Guid("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static Guid fixedRosterId =new Guid("CBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static Guid rosterGroupId;
    }
}
