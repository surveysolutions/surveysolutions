using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;


namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_updating_roster_group_by_question_and_roster_size_question_id_points_to_not_existing_question : QuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            groupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            rosterSizeQuestionId = Guid.Parse("11111111111111111111111111111111");

            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(groupId, responsibleId: responsibleId);
        }

        private void BecauseOf() =>
            exception = Catch.Exception(() =>
                questionnaire.UpdateGroup(groupId, responsibleId, "title",null, rosterSizeQuestionId, null, null, hideIfDisabled: false, isRoster: true,
                    rosterSizeSource: RosterSizeSourceType.Question, rosterFixedTitles: null, rosterTitleQuestionId: null));

        [NUnit.Framework.Test] public void should_throw_QuestionnaireException () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        [NUnit.Framework.Test] public void should_throw_exception_with_message_containting__roster__ () =>
            exception.Message.ToLower().ShouldContain("roster");

        [NUnit.Framework.Test] public void should_throw_exception_with_message_containting__question__ () =>
            exception.Message.ToLower().ShouldContain("question");

        [NUnit.Framework.Test] public void should_throw_exception_with_message_containting__missing__ () =>
            exception.Message.ToLower().ShouldContain("missing");

        private static Exception exception;
        private static Questionnaire questionnaire;
        private static Guid responsibleId;
        private static Guid groupId;
        private static Guid rosterSizeQuestionId;
    }
}