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
    internal class when_creating_roster_structure_factory_for_questionnarie_which_has_numeric_roster_group : QuestionnaireRosterStructureFactoryTestContext
    {
        Establish context = () =>
        {
            numericRosterGroupId = new Guid("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            numericQuestionId = new Guid("1111BBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            questionnarie = CreateQuestionnaireDocumentWithOneChapter(
                new NumericQuestion(){PublicKey = numericQuestionId, QuestionType = QuestionType.Numeric},
                new Group("Roster")
                {
                    PublicKey = numericRosterGroupId,
                    IsRoster = true,
                    RosterSizeSource = RosterSizeSourceType.Question,
                    RosterSizeQuestionId = numericQuestionId
                }
            );
            questionnaireRosterStructureFactory = CreateQuestionnaireRosterStructureFactory();
        };

        Because of = () =>
            questionnaireRosterStructure = questionnaireRosterStructureFactory.CreateQuestionnaireRosterStructure(questionnarie, 1);

        It should_contain_1_roster_scope = () =>
            questionnaireRosterStructure.RosterScopes.Count().ShouldEqual(1);

        It should_specify_numeric_question_id_as_id_of_roster_scope = () =>
            questionnaireRosterStructure.RosterScopes.Single().Key.SequenceEqual(new[] { numericQuestionId });

        It should_be_numeric_scope_type_for_numeric_roster_in_roster_scope = () =>
            questionnaireRosterStructure.RosterScopes.Single().Value.ScopeType.ShouldEqual(RosterScopeType.Numeric);

        private static QuestionnaireDocument questionnarie;
        private static QuestionnaireRosterStructureFactory questionnaireRosterStructureFactory;
        private static QuestionnaireRosterStructure questionnaireRosterStructure;
        private static Guid numericRosterGroupId;
        private static Guid numericQuestionId;
    }
}
