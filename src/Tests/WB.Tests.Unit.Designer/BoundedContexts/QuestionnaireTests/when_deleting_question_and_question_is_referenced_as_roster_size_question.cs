using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;


namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_deleting_question_and_question_is_referenced_as_roster_size_question : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            var chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            rosterSizeQuestionId = Guid.Parse("11111111111111111111111111111111");
            var rosterId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            rosterTitle = "Roster Title";

            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
            questionnaire.AddMultiOptionQuestion(rosterSizeQuestionId,chapterId,responsibleId);
            
            questionnaire.AddGroup(rosterId, title: rosterTitle, responsibleId: responsibleId, isRoster: true, rosterSourceType: RosterSizeSourceType.Question,
                rosterSizeQuestionId: rosterSizeQuestionId, rosterFixedTitles: null);
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                questionnaire.DeleteQuestion(rosterSizeQuestionId, responsibleId));

        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        It should_throw_exception_with_message_containting__roster__ = () =>
            exception.Message.ToLower().ShouldContain("roster");

        It should_throw_exception_with_message_containting__using__ = () =>
            exception.Message.ToLower().ShouldContain("source");

        It should_throw_exception_with_message_containting__referenced__ = () =>
            exception.Message.ToLower().ShouldContain("referenced");

        It should_throw_exception_with_message_containting_group_title = () =>
            exception.Message.ShouldContain(rosterTitle);

        private static Exception exception;
        private static string rosterTitle;
        private static Questionnaire questionnaire;
        private static Guid rosterSizeQuestionId;
        private static Guid responsibleId;
    }
}