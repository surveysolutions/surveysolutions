using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Attachments;
using WB.Core.BoundedContexts.Designer.Exceptions;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests.Attachments
{
    internal class when_deleting_attachment_without_premission_to_edit : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(questionnaireId: questionnaireId, responsibleId: ownerId);
            questionnaire.AddAttachment(Create.Command.AddAttachment(questionnaireId, attachmentId, ownerId));

            deleteAttachment = Create.Command.DeleteAttachment(questionnaireId, attachmentId, sharedPersonId);

            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () =>
            exception = Catch.Exception(() => questionnaire.DeleteAttachment(deleteAttachment));

        It should_throw_questionnaire_exception = () =>
            exception.ShouldBeOfExactType(typeof(QuestionnaireException));

        It should_throw_exception_with_type_DoesNotHavePermissionsForEdit = () =>
            ((QuestionnaireException)exception).ErrorType.ShouldEqual(DomainExceptionType.DoesNotHavePermissionsForEdit);

        private static Exception exception;
        private static DeleteAttachment deleteAttachment;
        private static Questionnaire questionnaire;
        private static readonly Guid ownerId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static readonly Guid sharedPersonId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid attachmentId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static EventContext eventContext;
    }
}