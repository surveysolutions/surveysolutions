using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.GenericSubdomains.Portable;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.QuestionnaireTests
{
    internal class when_questionnaire_have_numeric_question_which_is_triggering_roster_and_nested_roster_and_GetRosterGroupsByRosterSizeQuestion : QuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            rosterGroupId = new Guid("EBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            questionnaireDocument = CreateQuestionnaireDocumentWithOneChapter(
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
                                RosterSizeQuestionId = rosterSizeQuestionId,
                                IsRoster = true
                            }

                        }.ToReadOnlyCollection()
                });
            BecauseOf();
        }

        public void BecauseOf() =>
            nestedRosters = Create.Entity.PlainQuestionnaire(questionnaireDocument, 1).GetRosterGroupsByRosterSizeQuestion(rosterSizeQuestionId);

        [NUnit.Framework.Test] public void should_nestedRosters_has_2_elements () =>
            nestedRosters.Count().Should().Be(2);

        [NUnit.Framework.Test] public void should_nestedRosters_contain_rosterGroupId () =>
            nestedRosters.Should().Contain(rosterGroupId);

        [NUnit.Framework.Test] public void should_nestedRosters_contain_nestedRosterId () =>
            nestedRosters.Should().Contain(nestedRosterId);

        private static IEnumerable<Guid> nestedRosters;
        private static QuestionnaireDocument questionnaireDocument;
        private static Guid rosterSizeQuestionId = new Guid("ABBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static Guid nestedRosterId = new Guid("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static Guid rosterGroupId;
    }
}
