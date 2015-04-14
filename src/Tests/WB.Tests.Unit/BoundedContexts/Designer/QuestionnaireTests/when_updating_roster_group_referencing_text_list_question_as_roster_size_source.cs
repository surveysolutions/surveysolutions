using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireTests
{
    internal class when_updating_roster_group_referencing_text_list_question_as_roster_size_source : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.Apply(new NewQuestionAdded { QuestionType = QuestionType.TextList, PublicKey = rosterSizeQuestionId, GroupPublicKey = chapterId });
            questionnaire.Apply(new NewGroupAdded { PublicKey = groupId, ParentGroupPublicKey = chapterId });
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                questionnaire.UpdateGroup(
                    groupId, responsibleId, "title",null, rosterSizeQuestionId, "description", null,
                    isRoster: true, rosterSizeSource: RosterSizeSourceType.Question, rosterFixedTitles: new Tuple<string,string>[]{}, 
                    rosterTitleQuestionId: null));

        It should_not_fail = () =>
            exception.ShouldEqual(null);

        private static Exception exception;
        private static Questionnaire questionnaire;

        private static Guid chapterId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid groupId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static Guid responsibleId = Guid.Parse("CCCCCCCCCCCCCCCCDDDDDDDDDDDDDDDD");
        private static Guid rosterSizeQuestionId = Guid.Parse("11111111111111111111111111111111");
    }
}