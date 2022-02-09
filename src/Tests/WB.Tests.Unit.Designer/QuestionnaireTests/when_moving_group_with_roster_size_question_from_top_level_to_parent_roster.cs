using System;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_moving_group_with_roster_size_question_from_top_level_to_parent_roster : QuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);

            questionnaire.AddGroup(parentRosterId, chapterId, responsibleId: responsibleId, isRoster: true, rosterSourceType: RosterSizeSourceType.FixedTitles,
                rosterSizeQuestionId: null, rosterFixedTitles: new[] { new FixedRosterTitleItem("1", "1"), new FixedRosterTitleItem("2", "2") });
            
            questionnaire.AddGroup(groupToMoveId, responsibleId: responsibleId);
            questionnaire.AddNumericQuestion(rosterSizeQuestionId, groupToMoveId, responsibleId, isInteger : true);

            questionnaire.AddGroup(nestedRosterId, parentRosterId, responsibleId: responsibleId, isRoster: true, rosterSourceType: RosterSizeSourceType.Question,
                rosterSizeQuestionId: rosterSizeQuestionId, rosterFixedTitles: null);
            BecauseOf();
        }


        private void BecauseOf() => questionnaire.MoveGroup(groupToMoveId, parentRosterId, 0, responsibleId);

        [NUnit.Framework.Test] public void should_contains_group () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupToMoveId).Should().NotBeNull();

        [NUnit.Framework.Test] public void should_contains_group_with_parentRosterId_specified () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupToMoveId).GetParent().PublicKey.Should().Be(parentRosterId);

        private static Questionnaire questionnaire;
        private static Guid responsibleId = Guid.Parse("DDDD0000000000000000000000000000");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid parentRosterId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid rosterSizeQuestionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid groupToMoveId = Guid.Parse("21111111111111111111111111111111");
        private static Guid nestedRosterId = Guid.Parse("31111111111111111111111111111111");
    }
}
