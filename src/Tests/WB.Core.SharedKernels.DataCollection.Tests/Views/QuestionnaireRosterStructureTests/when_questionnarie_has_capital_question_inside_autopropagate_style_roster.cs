using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.SharedKernels.DataCollection.Tests.Views.QuestionnaireRosterStructureTests
{
    internal class when_questionnarie_has_capital_question_inside_autopropagate_style_roster : QuestionnaireRosterStructureTestContext
    {
        Establish context = () =>
        {
            capitalQuestionId = new Guid("CBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            autoPropagatedQuestionId = new Guid("EBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            rosterGroupId = new Guid("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            var roster = new Group("Roster") { Propagated = Propagate.AutoPropagated, PublicKey = rosterGroupId };
            roster.Children.Add(new NumericQuestion() { PublicKey = capitalQuestionId, Capital = true });

            questionnarie = CreateQuestionnaireDocument(
                new AutoPropagateQuestion()
                {
                    PublicKey = autoPropagatedQuestionId,
                    Triggers = new List<Guid> { rosterGroupId },
                    QuestionType = QuestionType.AutoPropagate
                },
                roster);
        };

        Because of = () =>
            questionnaireRosterStructure = new QuestionnaireRosterStructure(questionnarie, 1);

        It should_contain_roster_description_with_title_question_equal_to_capitalQuestionId = () =>
            questionnaireRosterStructure.RosterScopes[autoPropagatedQuestionId].HeadQuestionId.ShouldEqual(capitalQuestionId);

        private static QuestionnaireDocument questionnarie;
        private static QuestionnaireRosterStructure questionnaireRosterStructure;
        private static Guid capitalQuestionId;
        private static Guid rosterGroupId;
        private static Guid autoPropagatedQuestionId;
    }
}
