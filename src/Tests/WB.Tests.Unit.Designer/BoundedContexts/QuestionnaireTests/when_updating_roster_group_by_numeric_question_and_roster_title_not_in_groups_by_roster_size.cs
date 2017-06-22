using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;


namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_updating_roster_group_by_numeric_question_and_roster_title_not_in_groups_by_roster_size : QuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            var chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            parentGroupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            groupId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            rosterTitleQuestionId = Guid.Parse("1BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            rosterSizeQuestionId = Guid.Parse("2BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            rosterSizeSourceType = RosterSizeSourceType.Question;

            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
            questionnaire.AddGroup(groupId, chapterId, responsibleId: responsibleId);
            questionnaire.AddTextQuestion(rosterTitleQuestionId, chapterId, responsibleId);
            
            questionnaire.AddNumericQuestion(
                 rosterSizeQuestionId,
                chapterId,
                responsibleId,
                isInteger : true);
            questionnaire.AddGroup(parentGroupId, responsibleId: responsibleId);

            BecauseOf();
        }

        private void BecauseOf() =>
            exception = Catch.Exception(
                () =>
                    questionnaire.UpdateGroup(groupId: groupId, responsibleId: responsibleId, title: "title", variableName: null,
                        description: null, condition: null, hideIfDisabled: false, rosterSizeQuestionId: rosterSizeQuestionId,
                        isRoster: true, rosterSizeSource: rosterSizeSourceType, rosterFixedTitles: null,
                        rosterTitleQuestionId: rosterTitleQuestionId));

        [NUnit.Framework.Test] public void should_throw_QuestionnaireException () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        [NUnit.Framework.Test] public void should_throw_exception_with_message_containting__roster__ () =>
            exception.Message.ToLower().ShouldContain("roster");

        [NUnit.Framework.Test] public void should_throw_exception_with_message_containting__title__ () =>
            exception.Message.ToLower().ShouldContain("title");

        [NUnit.Framework.Test] public void should_throw_exception_with_message_containting__question__ () =>
            exception.Message.ToLower().ShouldContain("question");

        [NUnit.Framework.Test] public void should_throw_exception_with_message_containting__only__ () =>
            exception.Message.ToLower().ShouldContain("only");

        [NUnit.Framework.Test] public void should_throw_exception_with_message_containting__inside__ () =>
            exception.Message.ToLower().ShouldContain("inside");

        [NUnit.Framework.Test] public void should_throw_exception_with_message_containting__group__ () =>
            exception.Message.ToLower().ShouldContain("group");

        [NUnit.Framework.Test] public void should_throw_exception_with_message_containting__where__ () =>
            exception.Message.ToLower().ShouldContain("where");

        [NUnit.Framework.Test] public void should_throw_exception_with_message_containting__size__ () =>
            exception.Message.ToLower().ShouldContain("source");

        private static Questionnaire questionnaire;
        private static Guid responsibleId;
        private static Guid groupId;
        private static Guid rosterTitleQuestionId;
        private static Guid rosterSizeQuestionId;
        private static Guid parentGroupId;
        private static RosterSizeSourceType rosterSizeSourceType;
        private static Exception exception;
    }
}