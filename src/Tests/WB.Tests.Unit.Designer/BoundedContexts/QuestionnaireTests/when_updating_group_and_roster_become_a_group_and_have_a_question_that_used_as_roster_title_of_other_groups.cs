using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;


namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_updating_group_and_roster_become_a_group_and_have_a_question_that_used_as_roster_title_of_other_groups :
        QuestionnaireTestsContext
    {
        private [NUnit.Framework.OneTimeSetUp] public void context () {
            responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            var chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            var anotherRosterId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
            groupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            rosterSizeQuestionId = Guid.Parse("11111111111111111111111111111111");
            rosterTitleQuestionId = Guid.Parse("22222222222222222222222222222222");

            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);

            questionnaire.AddNumericQuestion(
                rosterSizeQuestionId,
                chapterId,
                responsibleId, isInteger: true);

            AddGroup(questionnaire: questionnaire, groupId: groupId, parentGroupId: chapterId, condition: null, responsibleId: responsibleId,
                rosterSizeQuestionId: rosterSizeQuestionId, isRoster: true, rosterSizeSource: RosterSizeSourceType.Question,
                rosterFixedTitles: null, rosterTitleQuestionId: null);
            
            questionnaire.AddTextQuestion(rosterTitleQuestionId,
                groupId,responsibleId);

            AddGroup(questionnaire: questionnaire, groupId: anotherRosterId, parentGroupId: chapterId, condition: null,
                responsibleId: responsibleId, rosterSizeQuestionId: rosterSizeQuestionId, isRoster: true,
                rosterSizeSource: RosterSizeSourceType.Question, rosterFixedTitles: null, rosterTitleQuestionId: rosterTitleQuestionId);
            BecauseOf();
        }

        private void BecauseOf() =>
            exception = Catch.Exception(() =>
                questionnaire.UpdateGroup(groupId: groupId, responsibleId: responsibleId, title: "title", variableName: null, rosterSizeQuestionId: null,
                    description: null, condition: null, hideIfDisabled: false, isRoster: false, rosterSizeSource: RosterSizeSourceType.Question,
                    rosterFixedTitles: null, rosterTitleQuestionId: null));

        [NUnit.Framework.Test] public void should_throw_QuestionnaireException () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        [NUnit.Framework.Test] public void should_throw_exception_with_message_containting__roster__ () =>
            exception.Message.ToLower().ShouldContain("roster");

        [NUnit.Framework.Test] public void should_throw_exception_with_message_containting__become__ () =>
            exception.Message.ToLower().ShouldContain("become");

        [NUnit.Framework.Test] public void should_throw_exception_with_message_containting__group__ () =>
            exception.Message.ToLower().ShouldContain("sub-section");

        [NUnit.Framework.Test] public void should_throw_exception_with_message_containting__contains__ () =>
            exception.Message.ToLower().ShouldContain("contains");

        [NUnit.Framework.Test] public void should_throw_exception_with_message_containting__title__ () =>
            exception.Message.ToLower().ShouldContain("title");

        [NUnit.Framework.Test] public void should_throw_exception_with_message_containting__question__ () =>
            exception.Message.ToLower().ShouldContain("question");

        private static Exception exception;
        private static Questionnaire questionnaire;
        private static Guid responsibleId;
        private static Guid groupId;
        private static Guid rosterSizeQuestionId;
        private static Guid rosterTitleQuestionId;
    }
}