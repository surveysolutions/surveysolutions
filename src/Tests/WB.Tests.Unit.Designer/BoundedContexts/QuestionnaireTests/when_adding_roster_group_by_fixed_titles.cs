using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;


namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_adding_roster_group_by_fixed_titles : QuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            var chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            parentGroupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            groupId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            rosterSizeSourceType = RosterSizeSourceType.FixedTitles;
            rosterFixedTitles = new[] { new FixedRosterTitleItem("1", rosterFixedTitle1), new FixedRosterTitleItem("2", rosterFixedTitle2) };

            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);

            questionnaire.AddTextQuestion(Guid.NewGuid(),
                chapterId,
                responsibleId);
            
            questionnaire.AddGroup(parentGroupId, responsibleId: responsibleId);
            BecauseOf();
        }

        private void BecauseOf() =>
            questionnaire.AddGroupAndMoveIfNeeded(groupId: groupId, responsibleId: responsibleId, title: "title", variableName: null,
                rosterSizeQuestionId: null, description: null, condition: null, hideIfDisabled: false, parentGroupId: parentGroupId,
                isRoster: true, rosterSizeSource: rosterSizeSourceType, rosterFixedTitles: rosterFixedTitles, rosterTitleQuestionId: null);


        [NUnit.Framework.Test] public void should_contains_group () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId).ShouldNotBeNull();

        [NUnit.Framework.Test] public void should_contains_group_with_GroupId_specified () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId)
                .PublicKey.ShouldEqual(groupId);

        [NUnit.Framework.Test] public void should_contains_group_with_RosterSizeSourceType_equal_to_specified_rosterSizeSourceType () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId)
                .RosterSizeSource.ShouldEqual(rosterSizeSourceType);

        [NUnit.Framework.Test] public void should_contains_group_with_RosterSizeQuestionId_equal_to_null () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId).RosterSizeQuestionId.ShouldBeNull();

        [NUnit.Framework.Test] public void should_contains_group_with_not_nullable_RosterFixedTitles () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId).FixedRosterTitles.ShouldNotBeNull();

        [NUnit.Framework.Test] public void should_contains_group_with_not_empty_RosterFixedTitles () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId).FixedRosterTitles.Length.ShouldNotEqual(0);

        [NUnit.Framework.Test] public void should_contains_group_with_RosterFixedTitles_specified () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId)
                .FixedRosterTitles.Select(x => x.Title).ShouldContainOnly(new[] { rosterFixedTitle1, rosterFixedTitle2 });

        [NUnit.Framework.Test] public void should_contains_group_with_RosterTitleQuestionId_equal_to_null () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId).RosterTitleQuestionId.ShouldBeNull();

        private static Questionnaire questionnaire;
        private static Guid responsibleId;
        private static Guid groupId;
        private static Guid parentGroupId;
        private static RosterSizeSourceType rosterSizeSourceType;
        private static FixedRosterTitleItem[] rosterFixedTitles;
        private static string rosterFixedTitle1 = "roster fixed title 1";
        private static string rosterFixedTitle2 = "roster fixed title 2";
    }
}