using System;
using System.Collections.Generic;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewTests.Variables
{
    public class Attachments : InterviewTestsContext
    {
        [Test]
        public void should_be_able_to_use_variable_as_attachment_name()
        {
            QuestionnaireDocument questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                children: new IComposite[]
                {
                    Create.Entity.Variable(Id.g1, VariableType.String, "v1", "\"attachment \" + sgl"),
                    Create.Entity.SingleQuestion(Id.g2, "sgl", options: new List<Answer>
                    {
                        Create.Entity.Answer("one", 1)
                    }),
                    Create.Entity.StaticText(Id.g3, attachmentName: "v1")
                });
            var attachment = Create.Entity.Attachment("hash");
            attachment.Name = "attachment 1";
            var attachmentId = Id.gB;
            attachment.AttachmentId = attachmentId;
            questionnaire.Attachments.Add(attachment);
            
            var interview = SetupStatefullInterview(AppDomainContext.Create().AssemblyLoadContext, questionnaire);
            interview.AnswerSingleOptionQuestion(Id.gA, Id.g2, RosterVector.Empty, DateTimeOffset.Now, 1);
            
            // Act
            var foundAttachmentId = interview.GetAttachmentForEntity(Create.Identity(Id.g3));
            
            // Assert
            Assert.That(foundAttachmentId, Is.EqualTo(attachmentId));
        }

        [Test]
        public void should_be_able_to_use_variable_from_outer_scope()
        {
            QuestionnaireDocument questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                children: new IComposite[]
                {
                    Create.Entity.Variable(Id.g1, VariableType.String, "v1", "\"attachment \" + sgl"),
                    Create.Entity.SingleQuestion(Id.g2, "sgl", options: new List<Answer>
                    {
                        Create.Entity.Answer("one", 1)
                    }),
                    Create.Entity.FixedRoster(
                        fixedTitles: new[]
                        {
                            Create.Entity.FixedTitle(1, "first instance")
                        },
                        children: new IComposite[]
                        {
                            Create.Entity.StaticText(Id.g3, attachmentName: "v1")
                        })
                });
            var attachment = Create.Entity.Attachment("hash");
            attachment.Name = "attachment 1";
            var attachmentId = Id.gB;
            attachment.AttachmentId = attachmentId;
            questionnaire.Attachments.Add(attachment);
            
            var interview = SetupStatefullInterview(AppDomainContext.Create().AssemblyLoadContext, questionnaire);
            interview.AnswerSingleOptionQuestion(Id.gA, Id.g2, RosterVector.Empty, DateTimeOffset.Now, 1);
            
            // Act
            var foundAttachmentId = interview.GetAttachmentForEntity(Create.Identity(Id.g3, Create.RosterVector(1)));
            
            // Assert
            Assert.That(foundAttachmentId, Is.EqualTo(attachmentId));
        }

        [Test]
        public void should_return_null_when_attached_variable_disabled()
        {
            QuestionnaireDocument questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                children: new IComposite[]
                {
                    Create.Entity.Group(enablementCondition: "sgl == 2", children: new []
                    {
                        Create.Entity.Variable(Id.g1, VariableType.String, "v1", "\"attachment \" + sgl"),
                    }),
                    Create.Entity.SingleQuestion(Id.g2, "sgl", options: new List<Answer>
                    {
                        Create.Entity.Answer("one", 1)
                    }),
                    Create.Entity.StaticText(Id.g3, attachmentName: "v1")
                });
            var attachment = Create.Entity.Attachment("hash");
            attachment.Name = "attachment 1";
            var attachmentId = Id.gB;
            attachment.AttachmentId = attachmentId;
            questionnaire.Attachments.Add(attachment);
            
            var interview = SetupStatefullInterview(AppDomainContext.Create().AssemblyLoadContext, questionnaire);
            interview.AnswerSingleOptionQuestion(Id.gA, Id.g2, RosterVector.Empty, DateTimeOffset.Now, 1);
            
            // Act
            Guid? foundAttachmentId = interview.GetAttachmentForEntity(Create.Identity(Id.g3));
            
            // Assert
            Assert.That(foundAttachmentId, Is.Null);

        }
    }
}
