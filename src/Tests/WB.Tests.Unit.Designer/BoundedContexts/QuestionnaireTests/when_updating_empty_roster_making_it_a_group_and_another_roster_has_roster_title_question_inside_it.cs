using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_updating_empty_roster_making_it_a_group_and_another_roster_has_roster_title_question_inside_it :
        QuestionnaireTestsContext
    {
        private Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.AddQuestion(Create.Event.NumericQuestionAdded
            (
                publicKey : rosterSizeQuestionId,
                isInteger : true,
                groupPublicKey : chapterId
            ));
            questionnaire.AddGroup(new NewGroupAdded { PublicKey = anotherRosterId, ParentGroupPublicKey = chapterId });
            questionnaire.MarkGroupAsRoster(new GroupBecameARoster(responsibleId: responsibleId, groupId: anotherRosterId));
            questionnaire.AddQuestion(Create.Event.NewQuestionAdded
            (
                publicKey : rosterTitleQuestionId,
                groupPublicKey : anotherRosterId,
                questionType : QuestionType.Text
            ));
            questionnaire.ChangeRoster(new RosterChanged(responsibleId: responsibleId, groupId: anotherRosterId){
                    RosterSizeQuestionId = rosterSizeQuestionId,
                    RosterSizeSource = RosterSizeSourceType.Question,
                    FixedRosterTitles =  null,
                    RosterTitleQuestionId =rosterTitleQuestionId 
                });

            questionnaire.AddGroup(new NewGroupAdded { PublicKey = rosterId, ParentGroupPublicKey = chapterId });
            questionnaire.MarkGroupAsRoster(new GroupBecameARoster(responsibleId:responsibleId, groupId: rosterId));
            questionnaire.ChangeRoster(new RosterChanged(responsibleId: responsibleId, groupId: rosterId){
                    RosterSizeQuestionId = rosterSizeQuestionId,
                    RosterSizeSource = RosterSizeSourceType.Question,
                    FixedRosterTitles =  null,
                    RosterTitleQuestionId =rosterTitleQuestionId 
                });
        };

        Because of = () => questionnaire.UpdateGroup(groupId: rosterId, responsibleId: responsibleId, title: "title",variableName:null, 
                    rosterSizeQuestionId: null, description: null, condition: null, hideIfDisabled: false, isRoster: false, 
                    rosterSizeSource: RosterSizeSourceType.Question, rosterFixedTitles: null, rosterTitleQuestionId: null);


        It should_contains_group = () =>
           questionnaire.QuestionnaireDocument.Find<IGroup>(rosterId).ShouldNotBeNull();

        It should_contains_group_with_GroupPublicKey_equal_to_roster_id = () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(rosterId).PublicKey.ShouldEqual(rosterId);

        It should_contains_group_with_IsRoster_equal_to_false = () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(rosterId).IsRoster.ShouldBeFalse();

        private static Questionnaire questionnaire;
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid rosterId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid anotherRosterId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static Guid rosterSizeQuestionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid rosterTitleQuestionId = Guid.Parse("22222222222222222222222222222222");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
    }
}