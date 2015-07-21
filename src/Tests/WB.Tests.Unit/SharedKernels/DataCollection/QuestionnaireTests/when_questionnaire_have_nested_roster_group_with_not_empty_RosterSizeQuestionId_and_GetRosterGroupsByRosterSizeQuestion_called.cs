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
    class when_questionnaire_have_nested_roster_group_with_not_empty_RosterSizeQuestionId_and_GetRosterGroupsByRosterSizeQuestion_called : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            rosterGroupId = new Guid("EBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            QuestionnaireDocument questionnaireDocument = CreateQuestionnaireDocumentWithOneChapter(new IComposite[]
            {
                new NumericQuestion() { PublicKey = rosterSizeQuestionId, IsInteger = true, QuestionType = QuestionType.Numeric},
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
                        }
                }
            });

            questionnaire = CreateQuestionnaire(Guid.NewGuid(), questionnaireDocument);
        };

        Because of = () =>
            rosterGroups = questionnaire.GetQuestionnaire().GetRosterGroupsByRosterSizeQuestion(rosterSizeQuestionId);

        It should_rosterGroups_not_be_empty = () =>
            rosterGroups.ShouldNotBeEmpty();

        It should_rosterGroups_have_only_1_roster_group = () =>
            rosterGroups.ShouldContainOnly(rosterGroupId);

        private static IEnumerable<Guid> rosterGroups;
        private static Questionnaire questionnaire;
        private static Guid rosterSizeQuestionId = new Guid("ABBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static Guid nestedGroupId = new Guid("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static Guid rosterGroupId;
    }
}
