using System;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Aggregates;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests.Attachments
{
    internal class when_cloning_questionnaire_with_attachment : QuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = CreateQuestionnaire(questionnaireId: questionnaireId, responsibleId: responsibleId);

            sourceQuestionnaire = Create.QuestionnaireDocument();
            sourceQuestionnaire.Attachments.Add(Create.Attachment(attachmentId: attachmentId, name: name, contentId: contentId));
            BecauseOf();
        }


        private void BecauseOf() => 
            questionnaire.CloneQuestionnaire("Title", false, responsibleId, clonedQuestionnaireId, sourceQuestionnaire);

        [NUnit.Framework.Test] public void should_contains_questionnaire_with_1_attachment () =>
            questionnaire.QuestionnaireDocument.Attachments.Count.Should().Be(1);

        [NUnit.Framework.Test] public void should_set_new_AttachmentId () =>
            questionnaire.QuestionnaireDocument.Attachments.First().AttachmentId.Should().NotBe(attachmentId);

        [NUnit.Framework.Test] public void should_set_original_Name () =>
            questionnaire.QuestionnaireDocument.Attachments.First().Name.Should().Be(name);

        [NUnit.Framework.Test] public void should_set_Content_Id () =>
            questionnaire.QuestionnaireDocument.Attachments.First().ContentId.Should().Be(contentId);

        private static Questionnaire questionnaire;
        private static QuestionnaireDocument sourceQuestionnaire;
        private static readonly Guid responsibleId = Guid.Parse("DDDD0000000000000000000000000000");
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid clonedQuestionnaireId = Guid.Parse("22222222222222222222222222222222");
        private static readonly Guid attachmentId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static readonly string name = "name";
        private static readonly string contentId = "content id";
    }
}
