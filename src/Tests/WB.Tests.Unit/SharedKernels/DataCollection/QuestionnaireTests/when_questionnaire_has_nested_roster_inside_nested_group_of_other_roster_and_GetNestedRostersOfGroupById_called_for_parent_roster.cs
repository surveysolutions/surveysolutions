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
    internal class when_questionnaire_has_nested_roster_inside_nested_group_of_other_roster_and_GetNestedRostersOfGroupById_called_for_parent_roster : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            rosterGroupId = new Guid("EBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            QuestionnaireDocument questionnaireDocument = CreateQuestionnaireDocumentWithOneChapter(new IComposite[]
            {
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
                            new Group("nested group")
                            {
                                Children = new List<IComposite>()
                                {

                                    new Group("nested roster")
                                    {
                                        PublicKey = nestedRosterId,
                                        RosterSizeSource = RosterSizeSourceType.FixedTitles,
                                        IsRoster = true
                                    }
                                }
                            }
                        }
                }
            });

            questionnaire = CreateQuestionnaire(Guid.NewGuid(), questionnaireDocument);
        };

        Because of = () =>
            nestedRosters = questionnaire.GetQuestionnaire().GetNestedRostersOfGroupById(rosterGroupId);

        It should_rosterGroups_not_be_empty = () =>
            nestedRosters.ShouldNotBeEmpty();

        It should_rosterGroups_have_only_1_roster_group = () =>
            nestedRosters.ShouldContainOnly(nestedRosterId);

        private static IEnumerable<Guid> nestedRosters;
        private static Questionnaire questionnaire;
        private static Guid rosterSizeQuestionId = new Guid("ABBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static Guid nestedRosterId = new Guid("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static Guid rosterGroupId;
    }
}
