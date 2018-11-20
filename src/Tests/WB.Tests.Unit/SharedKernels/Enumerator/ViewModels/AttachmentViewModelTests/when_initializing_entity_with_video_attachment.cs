using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.AttachmentViewModelTests
{
    class when_initializing_entity_with_video_attachment : AttachmentViewModelTestContext
    {
        [OneTimeSetUp]
        public void Context()
        {
            entityId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var attachmentContentId = "cccccc";
            attachment = Create.Entity.Attachment(attachmentContentId);
            attachmentContentMetadata = Create.Entity.AttachmentContentMetadata("video/mpg");
            
            attachmentContentData = Create.Entity.AttachmentContentData(new byte[] { 1, 2, 3 });
            this.questionnaireIdentity = Create.Entity.QuestionnaireIdentity(Guid.NewGuid());

            var questionnaireRepository = Setup.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireIdentity, _
                => _.GetAttachmentForEntity(entityId) == attachment);

            var interview = Mock.Of<IStatefulInterview>(i => i.QuestionnaireIdentity == questionnaireIdentity);
            var interviewRepository = Setup.StatefulInterviewRepository(interview);

            var attachmentStorage = Mock.Of<IAttachmentContentStorage>(s =>
                s.GetMetadata(attachmentContentId) == attachmentContentMetadata
                && s.GetContent(attachmentContentId) == attachmentContentData.Content
                && s.GetFileCacheLocation(attachmentContentId) == "cache");

            viewModel = Create.ViewModel.AttachmentViewModel(questionnaireRepository, interviewRepository, attachmentStorage);
            BecauseOf();
        }

        public void BecauseOf() => viewModel.Init("interview", Create.Identity(entityId, Empty.RosterVector), Create.Other.NavigationState());

        [Test] public void should_initialize_attachment_as_video() => viewModel.IsVideo.Should().BeTrue();
        [Test] public void should_not_initialize_attachment_as_image() => viewModel.IsImage.Should().BeFalse();

        [Test] public void should_initialize_video_contentPath() 
            => viewModel.ContentPath.Should()
                .BeEquivalentTo("cache");

        static AttachmentViewModel viewModel;
        private static Guid entityId;
        private static AttachmentContentMetadata attachmentContentMetadata;
        private static AttachmentContentData attachmentContentData;
        private Attachment attachment;
        private QuestionnaireIdentity questionnaireIdentity;
    }
}
