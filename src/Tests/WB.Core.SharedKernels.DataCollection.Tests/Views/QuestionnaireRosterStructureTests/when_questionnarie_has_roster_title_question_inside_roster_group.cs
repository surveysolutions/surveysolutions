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
    internal class when_questionnarie_has_roster_title_question_inside_roster_group : QuestionnaireRosterStructureTestContext
    {
        Establish context = () =>
        {
            rosterTitleQuestionId = new Guid("CBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            rosterSizeQuestionId = new Guid("EBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            rosterGroupId = new Guid("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            var roster = new Group("Roster") { PublicKey = rosterGroupId, IsRoster = true, RosterSizeQuestionId = rosterSizeQuestionId, RosterTitleQuestionId = rosterTitleQuestionId};
            roster.Children.Add(new NumericQuestion() { PublicKey = rosterTitleQuestionId, Capital = true });

            questionnarie = CreateQuestionnaireDocument(
                new NumericQuestion()
                {
                    PublicKey = rosterSizeQuestionId,
                    QuestionType = QuestionType.Numeric
                },
                roster);
        };

        Because of = () =>
            questionnaireRosterStructure = new QuestionnaireRosterStructure(questionnarie, 1);

        It should_contain_roster_description_with_title_question_equal_to_capitalQuestionId = () =>
            questionnaireRosterStructure.RosterScopes[rosterSizeQuestionId].HeadQuestionId.ShouldEqual(rosterTitleQuestionId);

        private static QuestionnaireDocument questionnarie;
        private static QuestionnaireRosterStructure questionnaireRosterStructure;
        private static Guid rosterTitleQuestionId;
        private static Guid rosterGroupId;
        private static Guid rosterSizeQuestionId;
    }
}
