using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Tests.Unit.Designer.BoundedContexts.AttachmentServiceTests
{
    internal class when_getting_attachments_by_questionnaire : AttachmentServiceTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {

            allAttachments.ForEach(attachment => attachmentMetaStorage.AttachmentMetas.Add(attachment));
            attachmentMetaStorage.SaveChanges();

            attachmentService = Create.AttachmentService(attachmentMetaStorage);
            BecauseOf();
        }

        private void BecauseOf() =>
            expectedAttachments = attachmentService.GetAttachmentsByQuestionnaire(questionnaireId);

        [NUnit.Framework.Test] public void should_return_2_specified_attachments () 
        {
            expectedAttachments.Count.Should().Be(2);
            expectedAttachments.All(x => x.QuestionnaireId == questionnaireId).Should().Be(true);
        }
        
        private static AttachmentService attachmentService;
        private static List<AttachmentMeta> expectedAttachments;
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");

        private static readonly AttachmentMeta[] allAttachments =
        {
            Create.AttachmentMeta(Guid.NewGuid(), "", questionnaireId),
            Create.AttachmentMeta(Guid.NewGuid(), "", Guid.NewGuid()),
            Create.AttachmentMeta(Guid.NewGuid(), "", questionnaireId)
        };
        
        private static readonly DesignerDbContext attachmentMetaStorage = Create.InMemoryDbContext();
    }
}
