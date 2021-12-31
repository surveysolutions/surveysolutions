using System;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_changing_roster_group_to_simple_group_but_question_inside_group_is_used_as_link_and_group_inside_other_roster : QuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            var rosterFixedTitles = new[] { new FixedRosterTitleItem("1", "1"), new FixedRosterTitleItem("2", "2") };
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);

            questionnaire.AddGroup(parentRosterId, chapterId, responsibleId: responsibleId, isRoster: true, rosterSourceType: RosterSizeSourceType.FixedTitles,
                rosterSizeQuestionId: null, rosterFixedTitles: rosterFixedTitles);

            questionnaire.AddGroup(groupToUpdateId,  parentRosterId, responsibleId: responsibleId, isRoster: true, rosterSourceType: RosterSizeSourceType.FixedTitles,
                rosterSizeQuestionId: null, rosterFixedTitles: rosterFixedTitles);

            questionnaire.AddNumericQuestion(questionUsedAsLinkId, groupToUpdateId, responsibleId, isInteger : true);

            questionnaire.AddSingleOptionQuestion(
                linkedQuestionInChapterId,
                chapterId,
                responsibleId,
                options: new Option[0],
                linkedToQuestionId : questionUsedAsLinkId);
            BecauseOf();
        }

        private void BecauseOf() =>
            questionnaire.UpdateGroup(groupToUpdateId, responsibleId, "title", null, null, "", "", false, false, RosterSizeSourceType.Question, null, null, 
                displayMode: RosterDisplayMode.Flat);

        [NUnit.Framework.Test] public void should_contains_group_with_groupToUpdateId_specified () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupToUpdateId).Should().NotBeNull();

        [NUnit.Framework.Test] public void should_contains_group_with_IsRoster_specified () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupToUpdateId).IsRoster.Should().BeFalse();

        private static Questionnaire questionnaire;
        private static Guid responsibleId = Guid.Parse("DDDD0000000000000000000000000000");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid parentRosterId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid questionUsedAsLinkId = Guid.Parse("11111111111111111111111111111111");
        private static Guid groupToUpdateId = Guid.Parse("21111111111111111111111111111111");
        private static Guid linkedQuestionInChapterId = Guid.Parse("31111111111111111111111111111111");
    }
}
