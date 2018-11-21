using System;
using FluentAssertions;
using Moq;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.AttachmentViewModelTests
{
    internal class when_initializing_entity_without_attachment : AttachmentViewModelTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            entityId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var questionnaireIdentity = Create.Entity.QuestionnaireIdentity(Guid.NewGuid());

            var questionnaireRepository = Setup.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireIdentity, _ => true);

            var interview = Mock.Of<IStatefulInterview>(i => i.QuestionnaireIdentity == questionnaireIdentity);
            var interviewRepository = Setup.StatefulInterviewRepository(interview);

            attachmentContentStorage = Mock.Of<IAttachmentContentStorage>();

            viewModel = Create.ViewModel.AttachmentViewModel(questionnaireRepository, interviewRepository, attachmentContentStorage);
        }

        public void BecauseOf() => viewModel.Init("interview", Create.Identity(entityId, Empty.RosterVector), Create.Other.NavigationState());


        [NUnit.Framework.Test] public void should_dont_call_attachment_content () =>
            Mock.Get(attachmentContentStorage).Verify(s => s.GetMetadata(Moq.It.IsAny<string>()), Times.Never());

        [NUnit.Framework.Test] public void should_initialize_image_flag_as_false () => 
            viewModel.IsImage.Should().BeFalse();

        [NUnit.Framework.Test] public void should_be_empty_attachment_content () => 
            viewModel.Content.Should().BeNull();


        static AttachmentViewModel viewModel;
        private static Guid entityId;
        private static IAttachmentContentStorage attachmentContentStorage;
    }
}

