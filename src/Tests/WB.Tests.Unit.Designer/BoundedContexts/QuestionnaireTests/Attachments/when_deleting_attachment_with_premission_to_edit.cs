using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Attachments;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire.Attachments;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests.Attachments
{
    internal class when_deleting_attachment_with_premission_to_edit : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(questionnaireId: questionnaireId, responsibleId: ownerId);
            questionnaire.AddSharedPerson(sharedPersonId, "email@email.com", ShareType.Edit, ownerId);
            questionnaire.AddAttachment(Create.Command.AddAttachment(questionnaireId, attachmentId, ownerId));

            deleteAttachment = Create.Command.DeleteAttachment(questionnaireId, attachmentId, sharedPersonId);

            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () => questionnaire.DeleteAttachment(deleteAttachment);

        It should_raise_MacroDeleted_event_with_EntityId_specified = () =>
            eventContext.GetSingleEvent<AttachmentDeleted>().AttachmentId.ShouldEqual(attachmentId);

        It should_raise_MacroDeleted_event_with_ResponsibleId_specified = () =>
            eventContext.GetSingleEvent<AttachmentDeleted>().ResponsibleId.ShouldEqual(sharedPersonId);

        private static DeleteAttachment deleteAttachment;
        private static Questionnaire questionnaire;
        private static readonly Guid ownerId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static readonly Guid sharedPersonId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid attachmentId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static EventContext eventContext;
    }
}