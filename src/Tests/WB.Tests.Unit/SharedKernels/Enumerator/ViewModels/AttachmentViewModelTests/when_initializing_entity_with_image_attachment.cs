using System;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;
using WB.Core.SharedKernels.Enumerator.Aggregates;

using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.AttachmentViewModelTests
{
    internal class when_initializing_entity_with_image_attachment : AttachmentViewModelTestContext
    {
        Establish context = () =>
        {
            entityId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var attachmentContentId = "cccccc";
            var attachment = Create.Entity.Attachment(attachmentContentId);
            attachmentContentMetadata = Create.Entity.AttachmentContentMetadata("image/png");
            attachmentContentData = Create.Entity.AttachmentContentData(new byte[] { 1, 2, 3 });
            var questionnaireIdentity = Create.Entity.QuestionnaireIdentity(Guid.NewGuid());

            var questionnaireRepository = Setup.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireIdentity, _ 
                => _.GetAttachmentForEntity(entityId) == attachment);

            var interview = Mock.Of<IStatefulInterview>(i => i.QuestionnaireIdentity == questionnaireIdentity);
            var interviewRepository = Setup.StatefulInterviewRepository(interview);

            var attachmentStorage = Mock.Of<IAttachmentContentStorage>(s =>
                s.GetMetadata(attachmentContentId) == attachmentContentMetadata
                && s.GetContent(attachmentContentId) == attachmentContentData.Content);

            viewModel = Create.ViewModel.AttachmentViewModel(questionnaireRepository, interviewRepository, attachmentStorage);
        };

        Because of = () => viewModel.Init("interview", new Identity(entityId, Empty.RosterVector));

        It should_initialize_attachment_as_image = () => viewModel.IsImage.ShouldBeTrue();

        It should_initialize_image_content = () => viewModel.Content.ShouldEqual(attachmentContentData.Content);

        static AttachmentViewModel viewModel;
        private static Guid entityId;
        private static AttachmentContentMetadata attachmentContentMetadata;
        private static AttachmentContentData attachmentContentData;
    }
}

