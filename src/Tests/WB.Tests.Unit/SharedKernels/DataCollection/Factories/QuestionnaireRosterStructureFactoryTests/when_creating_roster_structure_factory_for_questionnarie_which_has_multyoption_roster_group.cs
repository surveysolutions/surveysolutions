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
    internal class when_creating_roster_structure_factory_for_questionnarie_which_has_multyoption_roster_group : QuestionnaireRosterStructureFactoryTestContext
    {
        Establish context = () =>
        {
            multyOptionRosterGroupId = new Guid("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            multyOptionQuestionId = new Guid("1111BBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            questionnarie = CreateQuestionnaireDocumentWithOneChapter(
                new MultyOptionsQuestion() { PublicKey = multyOptionQuestionId, QuestionType = QuestionType.MultyOption },
                new Group("Roster")
                {
                    PublicKey = multyOptionRosterGroupId,
                    IsRoster = true,
                    RosterSizeSource = RosterSizeSourceType.Question,
                    RosterSizeQuestionId = multyOptionQuestionId
                }
            );
            questionnaireRosterStructureFactory = CreateQuestionnaireRosterStructureFactory();
        };

        Because of = () =>
            questionnaireRosterStructure = questionnaireRosterStructureFactory.CreateQuestionnaireRosterStructure(questionnarie, 1);

        It should_contain_1_roster_scope = () =>
            questionnaireRosterStructure.RosterScopes.Count().ShouldEqual(1);

        It should_specify_multyoption_question_id_as_id_of_roster_scope = () =>
            questionnaireRosterStructure.RosterScopes.Single().Key.SequenceEqual(new[] { multyOptionQuestionId });

        It should_be_multyoption_scope_type_for_multyoption_roster_in_roster_scope = () =>
            questionnaireRosterStructure.RosterScopes.Single().Value.ScopeType.ShouldEqual(RosterScopeType.MultyOption);

        private static QuestionnaireDocument questionnarie;
        private static QuestionnaireRosterStructureFactory questionnaireRosterStructureFactory;
        private static QuestionnaireRosterStructure questionnaireRosterStructure;
        private static Guid multyOptionRosterGroupId;
        private static Guid multyOptionQuestionId;
    }
}
