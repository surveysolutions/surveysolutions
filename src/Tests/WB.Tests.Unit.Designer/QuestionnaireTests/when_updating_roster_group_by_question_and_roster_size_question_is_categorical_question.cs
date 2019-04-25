using System;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;


namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_updating_roster_group_by_question_and_roster_size_question_is_categorical_question : QuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            var chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            groupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            rosterSizeQuestionId = Guid.Parse("11111111111111111111111111111111");
            rosterSizeSourceType = RosterSizeSourceType.Question;

            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);

            questionnaire.AddMultiOptionQuestion(rosterSizeQuestionId,chapterId,responsibleId);
            questionnaire.AddGroup(groupId, responsibleId: responsibleId);
            BecauseOf();
        }

        private void BecauseOf() =>
            questionnaire.UpdateGroup(groupId, responsibleId, "title",null, rosterSizeQuestionId, null, null, hideIfDisabled: false, isRoster: true,
                rosterSizeSource: rosterSizeSourceType, rosterFixedTitles: null, rosterTitleQuestionId: null, displayMode: RosterDisplayMode.Flat);



        [NUnit.Framework.Test] public void should_contains_group () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId).Should().NotBeNull();

        [NUnit.Framework.Test] public void should_contains_group_with_GroupId_specified () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId)
                .PublicKey.Should().Be(groupId);

        [NUnit.Framework.Test] public void should_contains_group_with_RosterSizeSourceType_equal_to_specified_rosterSizeSourceType () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId)
                .RosterSizeSource.Should().Be(rosterSizeSourceType);

        [NUnit.Framework.Test] public void should_contains_group_with_RosterSizeQuestionId_equal_to_specified_question_id () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId)
                .RosterSizeQuestionId.Should().Be(rosterSizeQuestionId);

        [NUnit.Framework.Test] public void should_contains_group_with_FixedRosterTitles_count_should_equal_0 () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId).FixedRosterTitles.Length.Should().Be(0);

        [NUnit.Framework.Test] public void should_contains_group_with_RosterTitleQuestionId_equal_to_null () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId).RosterTitleQuestionId.Should().BeNull();


        private static Questionnaire questionnaire;
        private static Guid responsibleId;
        private static Guid groupId;
        private static Guid rosterSizeQuestionId;
        private static RosterSizeSourceType rosterSizeSourceType;
    }
}
