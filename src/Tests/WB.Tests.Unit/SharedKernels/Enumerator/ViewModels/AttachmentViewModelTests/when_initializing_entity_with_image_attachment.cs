using System;
using System.IO;
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
        [OneTimeSetUp]
        public void context()
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
            BecauseOf();
        }

        public void BecauseOf() => viewModel.Init("interview", Create.Identity(entityId, Empty.RosterVector), Create.Other.NavigationState());

        [Test] public void should_initialize_attachment_as_image() => viewModel.IsImage.Should().BeTrue();

        [Test] public void should_initialize_image_content() => viewModel.Content.Should().BeEquivalentTo(attachmentContentData.Content);

        static AttachmentViewModel viewModel;
        private static Guid entityId;
        private static AttachmentContentMetadata attachmentContentMetadata;
        private static AttachmentContentData attachmentContentData;
    }
}

