using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Attachments;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire.Attachments;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests.Attachments
{
    internal class when_updating_attachment : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(questionnaireId: questionnaireId, responsibleId: responsibleId);
            questionnaire.AddAttachment(Create.Command.AddAttachment(questionnaireId, attachmentId, responsibleId));

            updateAttachment = Create.Command.UpdateAttachment(questionnaireId, attachmentId,responsibleId, name, fileName);

            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () => questionnaire.UpdateAttachment(updateAttachment);

        It should_raise_AttachmentUpdated_event_with_EntityId_specified = () =>
            eventContext.GetSingleEvent<AttachmentUpdated>().AttachmentId.ShouldEqual(attachmentId);

        It should_raise_AttachmentUpdated_event_with_ResponsibleId_specified = () =>
            eventContext.GetSingleEvent<AttachmentUpdated>().ResponsibleId.ShouldEqual(responsibleId);

        It should_raise_AttachmentUpdated_event_with_AttachmentName_specified = () =>
            eventContext.GetSingleEvent<AttachmentUpdated>().AttachmentName.ShouldEqual(name);

        It should_raise_AttachmentUpdated_event_with_AttachmentFileName_specified = () =>
            eventContext.GetSingleEvent<AttachmentUpdated>().AttachmentFileName.ShouldEqual(fileName);

        private static UpdateAttachment updateAttachment;
        private static Questionnaire questionnaire;
        private static readonly string name = "Attachment";
        private static readonly string fileName = "Attachment.PNG";
        private static readonly Guid responsibleId = Guid.Parse("DDDD0000000000000000000000000000");
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid attachmentId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static EventContext eventContext;
    }
}