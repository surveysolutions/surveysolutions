using System;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.QuestionnaireTests
{
    internal class when_disabling_questionnaire_for_delete_and_user_is_not_the_creator_of_this_questionaire : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(creatorId: responsibleId);
        };

        Because of = () =>
                exception = Catch.Exception(
                        () => questionnaire.DisableQuestionnaire(new DisableQuestionnaire(questionnaireVersion: 1, responsibleId: unknownUserId, questionnaireId: Guid.NewGuid())));

        It should_not_exception_be_null = () =>
            exception.ShouldNotBeNull();

        It should_exception_be_type_of_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        It should_throw_exception_with_message_containting__dont_have_permissions__ = () =>
            new[] { "don't", "have", "permissions" }.ShouldEachConformTo(keyword => exception.Message.ToLower().Contains(keyword));

        private static Guid responsibleId = Guid.Parse("11111111111111111111111111111111");
        private static Guid unknownUserId = Guid.Parse("22222222222222222222222222222222");
        private static Questionnaire questionnaire;
        private static Exception exception;
    }
}
