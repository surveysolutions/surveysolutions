using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;


namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_updating_group_and_group_become_a_roster_with_roster_inside : QuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
            questionnaire.AddNumericQuestion(rosterSizeQuestionId,chapterId,responsibleId,isInteger : true);
            questionnaire.AddGroup(groupId, chapterId, responsibleId: responsibleId);
            questionnaire.AddGroup(rosterId, groupId, responsibleId: responsibleId, isRoster:true);
        }

        private void BecauseOf() =>
                questionnaire.UpdateGroup(groupId: groupId, responsibleId: responsibleId, title: "title", variableName: null, rosterSizeQuestionId: rosterSizeQuestionId,
                    description: null, condition: null, hideIfDisabled: false, isRoster: true, rosterSizeSource: RosterSizeSourceType.Question,
                    rosterFixedTitles: null, rosterTitleQuestionId: null);

        [NUnit.Framework.Test] public void should_contains_group () =>
             questionnaire.QuestionnaireDocument.Find<IGroup>(groupId).ShouldNotBeNull();

        [NUnit.Framework.Test] public void should_contains_group_with_GroupId_specified () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId)
                .PublicKey.ShouldEqual(groupId);

        [NUnit.Framework.Test] public void should_contains_group_with_IsRoster_specified () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId)
                .IsRoster.ShouldBeTrue();

        private static Questionnaire questionnaire;
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid groupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid rosterId = Guid.Parse("11111111111111111111111111111111");
        private static Guid rosterSizeQuestionId = Guid.Parse("22222222222222222222222222222222");
    }
}