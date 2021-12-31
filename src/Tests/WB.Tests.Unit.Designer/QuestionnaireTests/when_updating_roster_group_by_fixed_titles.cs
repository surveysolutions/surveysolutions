using System;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;


namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_updating_roster_group_by_fixed_titles : QuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            var chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            groupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var questionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAB");

            rosterSizeSourceType = RosterSizeSourceType.FixedTitles;
            rosterFixedTitles = new[] { new FixedRosterTitleItem("1", rosterFixedTitle1), new FixedRosterTitleItem("2", rosterFixedTitle2) };

            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
            questionnaire.AddTextQuestion(questionId, chapterId, responsibleId);
            
            questionnaire.AddGroup( groupId, responsibleId: responsibleId);
            BecauseOf();
        }

        private void BecauseOf() =>
            questionnaire.UpdateGroup(groupId, responsibleId, "title",null, null, null, null, hideIfDisabled: false, isRoster: true,
                rosterSizeSource: rosterSizeSourceType, rosterFixedTitles: rosterFixedTitles, rosterTitleQuestionId: null, displayMode: RosterDisplayMode.Flat);


        [NUnit.Framework.Test] public void should_contains_group () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId).Should().NotBeNull();

        [NUnit.Framework.Test] public void should_contains_group_with_GroupId_specified () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId)
                .PublicKey.Should().Be(groupId);

        [NUnit.Framework.Test] public void should_contains_group_with_RosterSizeSourceType_equal_to_specified_rosterSizeSourceType () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId)
                .RosterSizeSource.Should().Be(rosterSizeSourceType);

        [NUnit.Framework.Test] public void should_contains_group_with_RosterSizeQuestionId_equal_to_null () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId).RosterSizeQuestionId.Should().BeNull();

        [NUnit.Framework.Test] public void should_contains_group_with_not_nullable_RosterFixedTitles () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId).FixedRosterTitles.Should().NotBeNull();

        [NUnit.Framework.Test] public void should_contains_group_with_RosterFixedTitles_that_have_2_items () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId).FixedRosterTitles.Should().NotBeEmpty();

        [NUnit.Framework.Test] public void should_contains_group_with_RosterFixedTitles_that_first_element_is_specified () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId).FixedRosterTitles[0].Title.Should().Be(rosterFixedTitle1);

        [NUnit.Framework.Test] public void should_contains_group_with_RosterFixedTitles_that_second_element_is_specified () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId).FixedRosterTitles[1].Title.Should().Be(rosterFixedTitle2);

        [NUnit.Framework.Test] public void should_contains_group_with_RosterTitleQuestionId_equal_to_null () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId).RosterTitleQuestionId.Should().BeNull();

        [NUnit.Framework.Test] public void should_contains_group_with_DisplayMode_equal_to_flat() =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId).DisplayMode.Should().Be(RosterDisplayMode.Flat);

        private static Questionnaire questionnaire;
        private static Guid responsibleId;
        private static Guid groupId;
        private static RosterSizeSourceType rosterSizeSourceType;
        private static FixedRosterTitleItem[] rosterFixedTitles;
        private static string rosterFixedTitle1 = "roster fixed title 1";
        private static string rosterFixedTitle2 = "roster fixed title 2";
    }
}
