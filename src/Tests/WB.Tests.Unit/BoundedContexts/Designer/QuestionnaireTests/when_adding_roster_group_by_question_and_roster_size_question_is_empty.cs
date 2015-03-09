using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireTests
{
    internal class when_adding_roster_group_by_question_and_roster_size_question_is_empty : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                questionnaire.AddGroupAndMoveIfNeeded(groupId, responsibleId, "title",null, null, null, null, null, isRoster: true,
                  rosterSizeSource: RosterSizeSourceType.Question, rosterFixedTitles: null, rosterTitleQuestionId: null));

        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        It should_throw_exception_with_message_containing__roster_question_empty__ = () =>
              new[] { "roster", "question", "empty" }.ShouldEachConformTo(
                     keyword => exception.Message.ToLower().TrimEnd('.').Contains(keyword));

        private static Exception exception;
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid  groupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Questionnaire questionnaire;
    }
}