using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;

namespace WB.Tests.Unit.SharedKernels.DataCollection.QuestionnaireTests
{
    internal class when_questionnaire_have_numeric_question_which_is_triggering_roster_and_nested_roster_and_GetRosterGroupsByRosterSizeQuestion : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            rosterGroupId = new Guid("EBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            QuestionnaireDocument questionnaireDocument = CreateQuestionnaireDocumentWithOneChapter(
                new NumericQuestion()
                {
                    PublicKey = rosterSizeQuestionId,
                    IsInteger = true,
                    QuestionType = QuestionType.Numeric
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

                        }
                });

            questionnaire = CreateQuestionnaire(Guid.NewGuid(), questionnaireDocument);
        };

        Because of = () =>
            nestedRosters = questionnaire.GetQuestionnaire().GetRosterGroupsByRosterSizeQuestion(rosterSizeQuestionId);

        It should_nestedRosters_has_2_elements = () =>
            nestedRosters.Count().ShouldEqual(2);

        It should_nestedRosters_contain_rosterGroupId = () =>
            nestedRosters.ShouldContain(rosterGroupId);

        It should_nestedRosters_contain_nestedRosterId = () =>
            nestedRosters.ShouldContain(nestedRosterId);

        private static IEnumerable<Guid> nestedRosters;
        private static Questionnaire questionnaire;
        private static Guid rosterSizeQuestionId = new Guid("ABBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static Guid nestedRosterId = new Guid("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static Guid rosterGroupId;
    }
}
