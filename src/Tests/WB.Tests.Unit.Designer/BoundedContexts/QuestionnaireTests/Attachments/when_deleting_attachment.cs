using System;
using System.Linq;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Attachments;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire.Attachments;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests.Attachments
{
    internal class when_deleting_attachment : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(questionnaireId: questionnaireId, responsibleId: responsibleId);
            questionnaire.AddOrUpdateAttachment(Create.Command.AddOrUpdateAttachment(questionnaireId, attachmentId, "", responsibleId, ""));

            deleteAttachment = Create.Command.DeleteAttachment(questionnaireId, attachmentId, responsibleId);
        };


        Because of = () => questionnaire.DeleteAttachment(deleteAttachment);

        It should_doesnt_contains_attachment_with_EntityId_specified = () =>
            questionnaire.QuestionnaireDocument.Attachments.FirstOrDefault(a => a.AttachmentId == attachmentId).ShouldBeNull();


        private static DeleteAttachment deleteAttachment;
        private static Questionnaire questionnaire;
        private static readonly Guid responsibleId = Guid.Parse("DDDD0000000000000000000000000000");
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid attachmentId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
    }
}