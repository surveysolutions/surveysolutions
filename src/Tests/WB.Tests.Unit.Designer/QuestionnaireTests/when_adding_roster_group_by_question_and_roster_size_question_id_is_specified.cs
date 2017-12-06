using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;


namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_adding_roster_group_by_question_and_roster_size_question_id_is_specified : QuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            var chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            groupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            rosterSizeQuestionId = Guid.Parse("11111111111111111111111111111111");

            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
            questionnaire.AddMultiOptionQuestion(rosterSizeQuestionId,chapterId,responsibleId);
            BecauseOf();
        }

        private void BecauseOf() =>
            questionnaire.AddGroupAndMoveIfNeeded(groupId, responsibleId, "title",null, rosterSizeQuestionId, null, null, false, null, isRoster: true,
                rosterSizeSource: RosterSizeSourceType.Question, rosterFixedTitles: null, rosterTitleQuestionId: null);


        [NUnit.Framework.Test] public void should_contains_group () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId).ShouldNotBeNull();

        [NUnit.Framework.Test] public void should_contains_group_with_GroupId_specified () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId)
                .PublicKey.ShouldEqual(groupId);

        [NUnit.Framework.Test] public void should_contains_group_with_RosterSizeQuestionId_equal_to_specified_question_id () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId)
                .RosterSizeQuestionId.ShouldEqual(rosterSizeQuestionId);

        private static Questionnaire questionnaire;
        private static Guid responsibleId;
        private static Guid rosterSizeQuestionId;
        private static Guid groupId;
    }
}