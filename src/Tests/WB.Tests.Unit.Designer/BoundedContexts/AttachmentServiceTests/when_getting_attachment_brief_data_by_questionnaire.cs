using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;

namespace WB.Tests.Unit.Designer.BoundedContexts.AttachmentServiceTests
{
    internal class when_getting_attachment_brief_data_by_questionnaire : AttachmentServiceTestContext
    {
        Establish context = () =>
        {
            attachmentMetaStorage.Store(Create.AttachmentMeta(attachment1Id, "hash1", questionnaireId: questionnaireId.FormatGuid()), attachment1Id);
            attachmentMetaStorage.Store(Create.AttachmentMeta(attachment2Id, "hash2", questionnaireId: questionnaireId.FormatGuid()), attachment2Id);

            Setup.ServiceLocatorForAttachmentService(attachmentContentStorage, attachmentMetaStorage);

            attachmentService = Create.AttachmentService();
        };

        Because of = () =>
            data = attachmentService.GetBriefAttachmentsMetaForQuestionnaire(questionnaireId);

        It should_return_2_pairs = () =>
            data.Count().ShouldEqual(2);

        It should_return_first_attachment_with_specified_values = () =>
        {
            data.First().AttachmentId.ShouldEqual(Guid.Parse(attachment1Id));
            data.First().AttachmentContentHash.ShouldEqual("hash1");
        };

        It should_return_second_attachment_with_specified_values = () =>
        {
            data.Second().AttachmentId.ShouldEqual(Guid.Parse(attachment2Id));
            data.Second().AttachmentContentHash.ShouldEqual("hash2");
        };

        private static IEnumerable<QuestionnaireAttachmentMeta> data;
        private static AttachmentService attachmentService;
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly string attachment1Id = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";
        private static readonly string attachment2Id = "BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB";
        private static readonly TestPlainStorage<AttachmentContent> attachmentContentStorage = new TestPlainStorage<AttachmentContent>();
        private static readonly TestPlainStorage<AttachmentMeta> attachmentMetaStorage = new TestPlainStorage<AttachmentMeta>();
    }
}