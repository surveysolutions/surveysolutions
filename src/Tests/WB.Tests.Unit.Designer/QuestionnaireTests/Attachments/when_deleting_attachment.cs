using System;
using System.Linq;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Attachments;


namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests.Attachments
{
    internal class when_deleting_attachment : QuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = CreateQuestionnaire(questionnaireId: questionnaireId, responsibleId: responsibleId);
            questionnaire.AddOrUpdateAttachment(Create.Command.AddOrUpdateAttachment(questionnaireId, attachmentId, "", responsibleId, ""));

            deleteAttachment = Create.Command.DeleteAttachment(questionnaireId, attachmentId, responsibleId);
            BecauseOf();
        }


        private void BecauseOf() => questionnaire.DeleteAttachment(deleteAttachment);

        [NUnit.Framework.Test] public void should_doesnt_contains_attachment_with_EntityId_specified () =>
            questionnaire.QuestionnaireDocument.Attachments.FirstOrDefault(a => a.AttachmentId == attachmentId).ShouldBeNull();


        private static DeleteAttachment deleteAttachment;
        private static Questionnaire questionnaire;
        private static readonly Guid responsibleId = Guid.Parse("DDDD0000000000000000000000000000");
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid attachmentId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
    }
}