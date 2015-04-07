using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireTests
{
    internal class when_cloning_roster_group_referencing_text_list_question_as_roster_size_source : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.Apply(new NewQuestionAdded { QuestionType = QuestionType.TextList, PublicKey = rosterSizeQuestionId, GroupPublicKey = chapterId });
            questionnaire.Apply(new NewGroupAdded { PublicKey = sourceGroupId, ParentGroupPublicKey = chapterId });
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                questionnaire.CloneGroupWithoutChildren(
                    groupId, responsibleId, "title", null, rosterSizeQuestionId, "description", null, chapterId,
                    isRoster: true, rosterSizeSource: RosterSizeSourceType.Question, rosterFixedTitles: new Tuple<decimal, string>[]{}, 
                    rosterTitleQuestionId: null, sourceGroupId: sourceGroupId, targetIndex: 0));

        It should_not_fail = () =>
            exception.ShouldEqual(null);

        private static Exception exception;
        private static Questionnaire questionnaire;

        private static Guid chapterId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid sourceGroupId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid groupId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static Guid responsibleId = Guid.Parse("CCCCCCCCCCCCCCCCDDDDDDDDDDDDDDDD");
        private static Guid rosterSizeQuestionId = Guid.Parse("11111111111111111111111111111111");
    }
}