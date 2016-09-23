using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_adding_roster_group_by_question_and_roster_size_question_id_is_not_specified : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            groupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            rosterSizeQuestionId = null;

            questionnaire = CreateQuestionnaire(responsibleId);
        };

        Because of = () =>
            questionnaire.AddGroupAndMoveIfNeeded(groupId, responsibleId, "title",null, rosterSizeQuestionId, null, null, false, null, false,
                RosterSizeSourceType.Question, rosterFixedTitles: null, rosterTitleQuestionId: null);


        It should_contains_group = () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId).ShouldNotBeNull();

        It should_contains_group_with_GroupId_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId)
                .PublicKey.ShouldEqual(groupId);

        It should_contains_group_with_IsRoster_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId)
                .IsRoster.ShouldEqual(false);

        private static Questionnaire questionnaire;
        private static Guid responsibleId;
        private static Guid? rosterSizeQuestionId;
        private static Guid groupId;
    }
}