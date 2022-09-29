using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.AttachmentViewModelTests
{
    internal class when_initializing_entity_with_image_attachment : AttachmentViewModelTestContext
    {
        [Test]
        public void should_initialize_attachment_as_image()
        {
            var entityId = Id.gA;
            var attachmentContentId = "cccccc";
            var attachment = Create.Entity.Attachment(attachmentContentId);
            var attachmentContentMetadata = Create.Entity.AttachmentContentMetadata("image/png");
            var attachmentContentData = Create.Entity.AttachmentContentData(new byte[] {1, 2, 3});
            var questionnaireIdentity = Create.Entity.QuestionnaireIdentity(Guid.NewGuid());

            var questionnaireRepository = SetUp.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireIdentity, _
                => _.GetAttachmentForEntity(entityId) == attachment 
                && _.GetAttachmentById(attachment.AttachmentId) == attachment);

            var entityIdentity = Create.Identity(entityId);
            var interview = Mock.Of<IStatefulInterview>(i => i.QuestionnaireIdentity == questionnaireIdentity &&
                                                             i.GetAttachmentForEntity(entityIdentity) == attachment.AttachmentId);
            var interviewRepository = SetUp.StatefulInterviewRepository(interview);

            var attachmentStorage = Mock.Of<IAttachmentContentStorage>(s =>
                s.GetMetadata(attachmentContentId) == attachmentContentMetadata
                && s.GetContent(attachmentContentId) == attachmentContentData.Content
                && s.GetPreviewContent(attachmentContentId) == attachmentContentData.Content);

            var viewModel =
                Create.ViewModel.AttachmentViewModel(questionnaireRepository, interviewRepository, attachmentStorage);

            // Act
            viewModel.Init("interview", entityIdentity, Create.Other.NavigationState());

            // Assert
            viewModel.IsImage.Should().BeTrue();
            viewModel.Image.Should().BeEquivalentTo(attachmentContentData.Content);
        }
    }
}

