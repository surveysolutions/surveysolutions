using System;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.QuestionnaireTests
{
    internal class when_import_questionnaire_and_base64string_of_assembly_is_empty : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(creatorId: responsibleId);
        };

        Because of = () =>
                exception = Catch.Exception(
                        () => questionnaire.ImportFromDesigner(Create.ImportFromDesignerCommand(responsibleId, string.Empty)));

        It should_not_exception_be_null = () =>
            exception.ShouldNotBeNull();

        It should_exception_be_type_of_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        It should_throw_exception_with_message_containting__dont_have_permissions__ = () =>
            new[] { "assembly", "empty" }.ShouldEachConformTo(keyword => exception.Message.ToLower().Contains(keyword));

        private static Guid responsibleId = Guid.Parse("11111111111111111111111111111111");
        private static Guid unknownUserId = Guid.Parse("22222222222222222222222222222222");
        private static Questionnaire questionnaire;
        private static Exception exception;
    }
}
