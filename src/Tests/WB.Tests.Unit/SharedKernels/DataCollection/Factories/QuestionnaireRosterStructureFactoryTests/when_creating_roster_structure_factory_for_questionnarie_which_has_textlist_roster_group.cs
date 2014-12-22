using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.DataCollection.Implementation.Factories;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Tests.Unit.SharedKernels.DataCollection.Factories.QuestionnaireRosterStructureFactoryTests
{
    internal class when_creating_roster_structure_factory_for_questionnarie_which_has_textlist_roster_group : QuestionnaireRosterStructureFactoryTestContext
    {
        Establish context = () =>
        {
            textlistRosterGroupId = new Guid("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            textlistQuestionId = new Guid("1111BBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            questionnarie = CreateQuestionnaireDocumentWithOneChapter(
                new TextListQuestion() { PublicKey = textlistQuestionId, QuestionType = QuestionType.TextList },
                new Group("Roster")
                {
                    PublicKey = textlistRosterGroupId,
                    IsRoster = true,
                    RosterSizeSource = RosterSizeSourceType.Question,
                    RosterSizeQuestionId = textlistQuestionId
                }
            );
            questionnaireRosterStructureFactory = CreateQuestionnaireRosterStructureFactory();
        };

        Because of = () =>
            questionnaireRosterStructure = questionnaireRosterStructureFactory.CreateQuestionnaireRosterStructure(questionnarie, 1);

        It should_contain_1_roster_scope = () =>
            questionnaireRosterStructure.RosterScopes.Count().ShouldEqual(1);

        It should_specify_textlist_question_id_as_id_of_roster_scope = () =>
            questionnaireRosterStructure.RosterScopes.Single().Key.SequenceEqual(new[] { textlistQuestionId });

        It should_be_textlist_scope_type_for_textlist_roster_in_roster_scope = () =>
            questionnaireRosterStructure.RosterScopes.Single().Value.ScopeType.ShouldEqual(RosterScopeType.TextList);

        private static QuestionnaireDocument questionnarie;
        private static QuestionnaireRosterStructureFactory questionnaireRosterStructureFactory;
        private static QuestionnaireRosterStructure questionnaireRosterStructure;
        private static Guid textlistRosterGroupId;
        private static Guid textlistQuestionId;
    }
}
