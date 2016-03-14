using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Attachments;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire.Attachments;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests.Attachments
{
    internal class when_adding_attachment : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(questionnaireId: questionnaireId, responsibleId: responsibleId);

            addAttachment = Create.Command.AddAttachment(questionnaireId, attachmentId, responsibleId);

            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () => questionnaire.AddAttachment(addAttachment);

        It should_raise_AttachmentAdded_event_with_EntityId_specified = () =>
            eventContext.GetSingleEvent<AttachmentAdded>().AttachmentId.ShouldEqual(attachmentId);

        It should_raise_AttachmentAdded_event_with_ResponsibleId_specified = () =>
            eventContext.GetSingleEvent<AttachmentAdded>().ResponsibleId.ShouldEqual(responsibleId);

        private static AddAttachment addAttachment;
        private static Questionnaire questionnaire;
        private static readonly Guid responsibleId = Guid.Parse("DDDD0000000000000000000000000000");
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid attachmentId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static EventContext eventContext;
    }
}