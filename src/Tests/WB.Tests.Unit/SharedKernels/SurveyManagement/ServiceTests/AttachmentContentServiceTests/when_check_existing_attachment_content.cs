﻿using Machine.Specifications;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.DataExport.CommentsExporterTests;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.AttachmentContentServiceTests
{
    class when_check_existing_attachment_content : CommentsExporterTestsContext
    {
        Establish context = () =>
        {
            var attachmentContentPlainStorage = new TestPlainStorage<AttachmentContent>();
            attachmentContentPlainStorage.Store(expectedContent, expectedContent.ContentHash);
            attachmentContentService = Create.Service.AttachmentContentService(attachmentContentPlainStorage);
        };

        Because of = () =>
            isAttachmentContentExists = attachmentContentService.HasAttachmentContent(expectedContent.ContentHash);

        It should_attachment_content_exists_in_plain_storage = () =>
            isAttachmentContentExists.ShouldBeTrue();

        private static AttachmentContentService attachmentContentService;
        private static readonly AttachmentContent expectedContent = Create.Entity.AttachmentContent_SurveyManagement();
        private static bool isAttachmentContentExists;
    }
}