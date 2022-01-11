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
    class when_questionnaire_have_nested_roster_group_with_not_empty_RosterSizeQuestionId_and_GetRosterGroupsByRosterSizeQuestion_called : QuestionnaireTestsContext
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
                    Children =
                        new List<IComposite>()
                        {
                            new Group("nested group")
                            {
                                PublicKey = nestedGroupId,
                                RosterSizeQuestionId = rosterSizeQuestionId,
                                IsRoster = false
                            }
                        }.ToReadOnlyCollection()
                }
            });
            BecauseOf();
        }

        public void BecauseOf() =>
            rosterGroups = Create.Entity.PlainQuestionnaire(questionnaireDocument, 1).GetRosterGroupsByRosterSizeQuestion(rosterSizeQuestionId);

        [NUnit.Framework.Test] public void should_rosterGroups_not_be_empty () =>
            rosterGroups.Should().NotBeEmpty();

        [NUnit.Framework.Test] public void should_rosterGroups_have_only_1_roster_group () =>
            rosterGroups.Should().BeEquivalentTo(rosterGroupId);

        private static IEnumerable<Guid> rosterGroups;
        private static QuestionnaireDocument questionnaireDocument;
        private static Guid rosterSizeQuestionId = new Guid("ABBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static Guid nestedGroupId = new Guid("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static Guid rosterGroupId;
    }
}
