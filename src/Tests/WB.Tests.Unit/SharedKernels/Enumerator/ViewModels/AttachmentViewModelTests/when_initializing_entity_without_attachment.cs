﻿using System;
using Machine.Specifications;
using Moq;
using Nito.AsyncEx.Synchronous;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.AttachmentViewModelTests
{
    internal class when_initializing_entity_without_attachment : AttachmentViewModelTestContext
    {
        Establish context = () =>
        {
            entityId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var questionnaireIdentity = Create.QuestionnaireIdentity(Guid.NewGuid());

            var questionnaireRepository = Setup.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireIdentity, _ => true);

            var interview = Mock.Of<IStatefulInterview>(i => i.QuestionnaireIdentity == questionnaireIdentity);
            var interviewRepository = Setup.StatefulInterviewRepository(interview);

            attachmentContentStorage = Mock.Of<IAttachmentContentStorage>();

            viewModel = Create.AttachmentViewModel(questionnaireRepository, interviewRepository, attachmentContentStorage);
        };

        Because of = () => viewModel.InitAsync("interview", new Identity(entityId, Empty.RosterVector)).WaitAndUnwrapException();


        It should_dont_call_attachment_content = () =>
            Mock.Get(attachmentContentStorage).Verify(s => s.GetMetadata(Moq.It.IsAny<string>()), Times.Never());

        It should_initialize_image_flag_as_false = () => 
            viewModel.IsImage.ShouldBeFalse();

        It should_be_empty_attachment_content = () => 
            viewModel.Content.ShouldBeNull();


        static AttachmentViewModel viewModel;
        private static Guid entityId;
        private static IAttachmentContentStorage attachmentContentStorage;
    }
}

