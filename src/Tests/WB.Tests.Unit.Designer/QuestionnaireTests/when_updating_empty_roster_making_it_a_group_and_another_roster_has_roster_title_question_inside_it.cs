using System;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;


namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_updating_empty_roster_making_it_a_group_and_another_roster_has_roster_title_question_inside_it : QuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
            questionnaire.AddNumericQuestion(rosterSizeQuestionId,isInteger : true,parentId: chapterId,responsibleId:responsibleId);
            questionnaire.AddGroup(anotherRosterId,  chapterId, responsibleId: responsibleId, isRoster: true, rosterSourceType: RosterSizeSourceType.Question,
                rosterSizeQuestionId: rosterSizeQuestionId, rosterFixedTitles: null);
            
            questionnaire.AddTextQuestion(rosterTitleQuestionId,anotherRosterId, responsibleId);

            //update roster title question
           /* questionnaire.ChangeRoster(new RosterChanged(responsibleId: responsibleId, groupId: anotherRosterId){
                    RosterSizeQuestionId = rosterSizeQuestionId,
                    RosterSizeSource = RosterSizeSourceType.Question,
                    FixedRosterTitles =  null,
                    RosterTitleQuestionId =rosterTitleQuestionId 
                });*/

            questionnaire.AddGroup(rosterId,chapterId, responsibleId: responsibleId, isRoster: true, rosterSourceType: RosterSizeSourceType.Question,
                rosterSizeQuestionId: rosterSizeQuestionId, rosterFixedTitles: null);
            //update roster title question
            /*questionnaire.ChangeRoster(new RosterChanged(responsibleId: responsibleId, groupId: rosterId){
                    RosterSizeQuestionId = rosterSizeQuestionId,
                    RosterSizeSource = RosterSizeSourceType.Question,
                    FixedRosterTitles =  null,
                    RosterTitleQuestionId =rosterTitleQuestionId 
                });*/
            BecauseOf();
        }

        private void BecauseOf() => questionnaire.UpdateGroup(groupId: rosterId, responsibleId: responsibleId, title: "title",variableName:null, 
                    rosterSizeQuestionId: null, description: null, condition: null, hideIfDisabled: false, isRoster: false, 
                    rosterSizeSource: RosterSizeSourceType.Question, rosterFixedTitles: null, rosterTitleQuestionId: null, displayMode: RosterDisplayMode.Flat);


        [NUnit.Framework.Test] public void should_contains_group () =>
           questionnaire.QuestionnaireDocument.Find<IGroup>(rosterId).Should().NotBeNull();

        [NUnit.Framework.Test] public void should_contains_group_with_GroupPublicKey_equal_to_roster_id () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(rosterId).PublicKey.Should().Be(rosterId);

        [NUnit.Framework.Test] public void should_contains_group_with_IsRoster_equal_to_false () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(rosterId).IsRoster.Should().BeFalse();

        private static Questionnaire questionnaire;
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid rosterId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid anotherRosterId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static Guid rosterSizeQuestionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid rosterTitleQuestionId = Guid.Parse("22222222222222222222222222222222");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
    }
}
