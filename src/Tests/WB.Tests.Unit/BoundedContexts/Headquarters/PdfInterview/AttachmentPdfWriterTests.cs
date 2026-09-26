using System;
using MigraDocCore.DocumentObjectModel;
using MigraDocCore.DocumentObjectModel.MigraDoc.DocumentObjectModel.Shapes;
using Moq;
using NUnit.Framework;
using SixLabors.ImageSharp.PixelFormats;
using WB.Core.BoundedContexts.Headquarters.PdfInterview;
using WB.Core.BoundedContexts.Headquarters.PdfInterview.PdfWriters;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.PdfInterview;

[TestOf(typeof(AttachmentPdfWriter))]
public class AttachmentPdfWriterTests
{
    [Test]
    public void when_attachment_image_format_is_not_supported_should_not_throw()
    {
        ImageSource.ImageSourceImpl = new ImageSharpSource<Rgba32>();

        var attachmentInfo = Create.Entity.Attachment("content-id");
        var interview = Mock.Of<IStatefulInterview>();
        var questionnaire = Mock.Of<IQuestionnaire>(x => x.GetAttachmentById(attachmentInfo.AttachmentId) == attachmentInfo);
        var attachmentContentService = Mock.Of<IAttachmentContentService>(x =>
            x.GetAttachmentContent("content-id") == new AttachmentContent
            {
                ContentHash = "content-id",
                ContentType = "image/heic",
                FileName = "image.heic",
                Content = new byte[] { 1, 2, 3, 4 }
            });

        var writer = new AttachmentPdfWriter(attachmentInfo.AttachmentId, interview, questionnaire, attachmentContentService);
        var paragraph = new Paragraph();

        Assert.DoesNotThrow(() => writer.Write(paragraph));
    }
}
