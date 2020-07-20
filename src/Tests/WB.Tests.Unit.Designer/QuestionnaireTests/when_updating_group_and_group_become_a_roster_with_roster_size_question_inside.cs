using System;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;


namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_updating_group_and_group_become_a_roster_with_roster_size_question_inside : QuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
            questionnaire.AddGroup(groupId,chapterId, responsibleId: responsibleId);
            questionnaire.AddNumericQuestion(rosterSizeQuestionId,groupId,responsibleId,isInteger : true);
            
            questionnaire.AddGroup(rosterId,chapterId, responsibleId: responsibleId, isRoster: true, rosterSourceType: RosterSizeSourceType.Question,
                rosterSizeQuestionId: rosterSizeQuestionId, rosterFixedTitles: null);
            BecauseOf();

        }


        private void BecauseOf() =>
                questionnaire.UpdateGroup(groupId: groupId, responsibleId: responsibleId, title: "title", variableName: null, rosterSizeQuestionId: rosterSizeQuestionId,
                    description: null, condition: null, hideIfDisabled: false, isRoster: true, rosterSizeSource: RosterSizeSourceType.Question,
                    rosterFixedTitles: null, rosterTitleQuestionId: null, displayMode: RosterDisplayMode.Flat, false);

        [NUnit.Framework.Test] public void should_contains_group () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId).Should().NotBeNull();

        [NUnit.Framework.Test] public void should_contains_group_with_GroupId_specified () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId)
                .PublicKey.Should().Be(groupId);

        [NUnit.Framework.Test] public void should_contains_group_with_IsRoster_specified () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId)
                .IsRoster.Should().BeTrue();

        [NUnit.Framework.Test] public void should_contains_group_with_IsPlainMode_specified() =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId)
                .DisplayMode.Should().Be(RosterDisplayMode.Flat);

        private static Questionnaire questionnaire;
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid groupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid rosterId = Guid.Parse("11111111111111111111111111111112");
        private static Guid rosterSizeQuestionId = Guid.Parse("22222222222222222222222222222222");
    }
}
