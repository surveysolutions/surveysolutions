using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;


namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_moving_group_with_roster_size_question_to_roster : QuestionnaireTestsContext
    {
         [NUnit.Framework.OneTimeSetUp] public void context () {
             questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
             questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
             questionnaire.AddGroup(groupId, chapterId, responsibleId: responsibleId);
             questionnaire.AddNumericQuestion(rosterSizeQuestionId,groupId, responsibleId,isInteger: true);
             questionnaire.AddGroup(rosterId,chapterId, responsibleId: responsibleId, isRoster: true, rosterSourceType: RosterSizeSourceType.Question,
                rosterSizeQuestionId: rosterSizeQuestionId, rosterFixedTitles: null);
            BecauseOf();
        }

        private void BecauseOf() =>
             exception = Catch.Exception(() =>
                 questionnaire.MoveGroup(groupId, rosterId, 0, responsibleId));

        [NUnit.Framework.Test] public void should_throw_QuestionnaireException () =>
             exception.ShouldBeOfExactType<QuestionnaireException>();

        [NUnit.Framework.Test] public void should_throw_exception_with_message_containting__contains__ () =>
             exception.Message.ToLower().ShouldContain("contains");

        [NUnit.Framework.Test] public void should_throw_exception_with_message_containting__roster__ () =>
             exception.Message.ToLower().ShouldContain("roster");

        [NUnit.Framework.Test] public void should_throw_exception_with_message_containting__size__ () =>
             exception.Message.ToLower().ShouldContain("source");

        [NUnit.Framework.Test] public void should_throw_exception_with_message_containting__question__ () =>
             exception.Message.ToLower().ShouldContain("question");

        private static Questionnaire questionnaire;
        private static Guid responsibleId = Guid.Parse("DDDD0000000000000000000000000000");
        private static Guid groupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid rosterId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static Guid rosterSizeQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Exception exception;
    }
}