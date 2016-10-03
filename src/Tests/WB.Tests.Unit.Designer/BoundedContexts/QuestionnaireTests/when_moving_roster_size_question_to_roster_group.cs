using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_moving_roster_size_question_to_roster_group : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            rosterGroupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            targetRosterGroupId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");

            rosterSizeQuestionId = Guid.Parse("11111111111111111111111111111111");

            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(new NewGroupAdded { PublicKey = chapterId });


            AddGroup(questionnaire: questionnaire, groupId: targetRosterGroupId, parentGroupId: chapterId, condition: null,
                responsibleId: responsibleId, rosterSizeQuestionId: null, isRoster: true, rosterSizeSource: RosterSizeSourceType.FixedTitles,
                rosterTitleQuestionId: null, rosterFixedTitles: new[] { new FixedRosterTitleItem("1", "fixed title 1"), new FixedRosterTitleItem("2", "test 2") });

            questionnaire.AddQuestion(Create.Event.NumericQuestionAdded
            (
                publicKey: rosterSizeQuestionId,
                isInteger: true,
                groupPublicKey: targetRosterGroupId
            ));
            
            AddGroup(questionnaire: questionnaire, groupId: rosterGroupId, parentGroupId: targetRosterGroupId, condition: null,
                responsibleId: responsibleId, rosterSizeQuestionId: rosterSizeQuestionId, isRoster: true);
        };


        Because of = () =>
            questionnaire.MoveQuestion(rosterSizeQuestionId, chapterId, targetIndex: 0, responsibleId: responsibleId);

        It should_contains_question = () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(rosterSizeQuestionId).ShouldNotBeNull();

        It should_contains_question_with_GroupId_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(rosterSizeQuestionId)
           .PublicKey.ShouldEqual(rosterSizeQuestionId);

        It should_contains_question_with_chapterId_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(rosterSizeQuestionId)
            .GetParent().PublicKey.ShouldEqual(chapterId);


        private static Questionnaire questionnaire;
        private static Guid responsibleId;
        private static Guid rosterGroupId;
        private static Guid targetRosterGroupId;
        private static Guid chapterId;
        private static Guid rosterSizeQuestionId;
    }
}