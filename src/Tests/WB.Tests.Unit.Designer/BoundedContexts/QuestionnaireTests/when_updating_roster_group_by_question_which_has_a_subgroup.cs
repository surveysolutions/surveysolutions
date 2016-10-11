using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_updating_roster_group_by_question_which_has_a_subgroup : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(new NewGroupAdded { PublicKey = chapterId });

            questionnaire.AddDefaultTypeQuestionAdnMoveIfNeeded(Create.Command.AddDefaultTypeQuestion(questionnaire.Id, rosterSizeQuestionId, "title", responsibleId, chapterId));
            questionnaire.UpdateNumericQuestion(Create.Command.UpdateNumericQuestion(questionnaire.Id, rosterSizeQuestionId, responsibleId, "title", isInteger:true));
            
            questionnaire.AddGroup(new NewGroupAdded { PublicKey = groupId, ParentGroupPublicKey = chapterId});
            questionnaire.AddGroup(new NewGroupAdded { PublicKey = subGroupId, ParentGroupPublicKey = groupId });
        };

        Because of = () => questionnaire.UpdateGroup(groupId, responsibleId, "title",null, rosterSizeQuestionId, null, null, hideIfDisabled: false, isRoster: true,
                    rosterSizeSource: RosterSizeSourceType.Question, rosterFixedTitles: null, rosterTitleQuestionId: null);


        It should_contains_group = () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId);

        It should_contains_group_with_GroupId_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId)
                .PublicKey.ShouldEqual(groupId);

        private static Questionnaire questionnaire;
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid groupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid rosterSizeQuestionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid subGroupId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
    }
}