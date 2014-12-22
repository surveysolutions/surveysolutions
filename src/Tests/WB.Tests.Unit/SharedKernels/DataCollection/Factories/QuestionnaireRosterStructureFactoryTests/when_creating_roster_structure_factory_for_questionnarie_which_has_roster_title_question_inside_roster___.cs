using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.DataCollection.Implementation.Factories;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Tests.Unit.SharedKernels.DataCollection.Factories.QuestionnaireRosterStructureFactoryTests
{
    internal class when_creating_roster_structure_factory_for_questionnarie_which_has_roster_title_question_inside_roster_group : QuestionnaireRosterStructureFactoryTestContext
    {
        Establish context = () =>
        {
            rosterTitleQuestionId = new Guid("CBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            rosterSizeQuestionId = new Guid("EBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            rosterGroupId = new Guid("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            questionnarie = CreateQuestionnaireDocumentWithOneChapter(

                new NumericQuestion { PublicKey = rosterSizeQuestionId },

                new Group("Roster")
                {
                    PublicKey = rosterGroupId,
                    IsRoster = true,
                    RosterSizeQuestionId = rosterSizeQuestionId,
                    RosterTitleQuestionId = rosterTitleQuestionId,

                    Children = new List<IComposite>
                    {
                        new NumericQuestion { PublicKey = rosterTitleQuestionId, Capital = true },
                    }
                }
            );
            questionnaireRosterStructureFactory = CreateQuestionnaireRosterStructureFactory();
        };

        Because of = () =>
            questionnaireRosterStructure = questionnaireRosterStructureFactory.CreateQuestionnaireRosterStructure(questionnarie, 1);

        It should_contain_1_roster_scope = () =>
            questionnaireRosterStructure.RosterScopes.Count().ShouldEqual(1);

        It should_specify_roster_size_question_id_as_id_of_roster_scope = () =>
            questionnaireRosterStructure.RosterScopes.Single().Key.SequenceEqual(new[] { rosterSizeQuestionId });

        It should_specify_id_of_roster_title_question_as_roster_title_question_id_for_roster_id_in_roster_scope = () =>
            questionnaireRosterStructure.RosterScopes.Single().Value
                .RosterIdToRosterTitleQuestionIdMap[rosterGroupId].QuestionId.ShouldEqual(rosterTitleQuestionId);

        private static QuestionnaireDocument questionnarie;
        private static QuestionnaireRosterStructureFactory questionnaireRosterStructureFactory;
        private static QuestionnaireRosterStructure questionnaireRosterStructure;
        private static Guid rosterTitleQuestionId;
        private static Guid rosterGroupId;
        private static Guid rosterSizeQuestionId;
    }
}
