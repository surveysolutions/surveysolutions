using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.Factories.QuestionnaireRosterStructureTests
{
    internal class when_getting_roster_scope_descriptios_for_questionnarie_which_has_textlist_roster_group : QuestionnaireRosterStructureTestContext
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
            
            rostrerStructureService = new RosterStructureService();
        };

        Because of = () =>
            rosterScopes = rostrerStructureService.GetRosterScopes(questionnarie);

        It should_contain_1_roster_scope = () =>
            rosterScopes.Count().ShouldEqual(1);

        It should_specify_textlist_question_id_as_id_of_roster_scope = () =>
            rosterScopes.Single().Key.SequenceEqual(new[] { textlistQuestionId });

        It should_be_textlist_scope_type_for_textlist_roster_in_roster_scope = () =>
            rosterScopes.Single().Value.Type.ShouldEqual(RosterScopeType.TextList);

        private static QuestionnaireDocument questionnarie;
        private static IRostrerStructureService rostrerStructureService;
        private static Dictionary<ValueVector<Guid>, RosterScopeDescription> rosterScopes;
        private static Guid textlistRosterGroupId;
        private static Guid textlistQuestionId;
    }
}
