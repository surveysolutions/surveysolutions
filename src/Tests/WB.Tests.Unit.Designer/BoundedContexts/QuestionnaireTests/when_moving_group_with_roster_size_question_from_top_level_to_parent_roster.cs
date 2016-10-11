using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_moving_group_with_roster_size_question_from_top_level_to_parent_roster : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);

            questionnaire.AddGroup(parentRosterId, chapterId, responsibleId: responsibleId);
            questionnaire.MarkGroupAsRoster(new GroupBecameARoster(responsibleId, parentRosterId));
            questionnaire.ChangeRoster(new RosterChanged(responsibleId: responsibleId, groupId: parentRosterId){
                    RosterSizeQuestionId = null,
                    RosterSizeSource = RosterSizeSourceType.FixedTitles,
                    FixedRosterTitles = new[] { new FixedRosterTitle(1, "1"), new FixedRosterTitle(2, "2") },
                    RosterTitleQuestionId = null 
                });

            questionnaire.AddGroup(groupToMoveId, responsibleId: responsibleId);
            questionnaire.AddNumericQuestion(rosterSizeQuestionId, groupToMoveId, responsibleId, isInteger : true);

            questionnaire.AddGroup(nestedRosterId, parentRosterId, responsibleId: responsibleId);
            questionnaire.MarkGroupAsRoster(new GroupBecameARoster(responsibleId, nestedRosterId));
            questionnaire.ChangeRoster(new RosterChanged(responsibleId: responsibleId, groupId: nestedRosterId){
                    RosterSizeQuestionId = rosterSizeQuestionId,
                    RosterSizeSource = RosterSizeSourceType.Question,
                    FixedRosterTitles =  null,
                    RosterTitleQuestionId =null 
                });
        };


        Because of = () => questionnaire.MoveGroup(groupToMoveId, parentRosterId, 0, responsibleId);

        It should_contains_group = () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupToMoveId);

        It should_contains_group_with_parentRosterId_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupToMoveId).GetParent().PublicKey.ShouldEqual(parentRosterId);

        private static Questionnaire questionnaire;
        private static Guid responsibleId = Guid.Parse("DDDD0000000000000000000000000000");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid parentRosterId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid rosterSizeQuestionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid groupToMoveId = Guid.Parse("21111111111111111111111111111111");
        private static Guid nestedRosterId = Guid.Parse("31111111111111111111111111111111");
    }
}
