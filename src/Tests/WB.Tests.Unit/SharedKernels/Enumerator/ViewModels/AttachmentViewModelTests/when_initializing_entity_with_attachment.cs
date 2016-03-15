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
using WB.Core.SharedKernels.Enumerator.Entities.Interview;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.AttachmentViewModelTests
{
    internal class when_initializing_entity_with_attachment : AttachmentViewModelTestContext
    {
        Establish context = () =>
        {
            entityId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var attachmentId = Guid.Parse("BBBAAAAAAAAAAAAAAAAAAAAAAAAAAAAA").FormatGuid();
            var attachmentContentId = "cccccc";
            var attachment = new Attachment();
            var attachmentMetadata = new AttachmentMetadata() { AttachmentContentId = attachmentContentId };

            var questionnaireMock = Mock.Of<IQuestionnaire>(_
                => _.GetAttachmentForEntity(entityId) == attachment);
            var questionnaireRepository = new Mock<IPlainQuestionnaireRepository>();
            questionnaireRepository.SetReturnsDefault(questionnaireMock);

            var interview = Mock.Of<IStatefulInterview>();
            var interviewRepository = new Mock<IStatefulInterviewRepository>();
            interviewRepository.SetReturnsDefault(interview);

            attachmentContent = new byte[]{ 1, 2, 3 };
            var attachmentStorage = Mock.Of<IQuestionnaireAttachmentStorage>(s =>
                s.GetAttachmentAsync(attachmentId) == Task.FromResult(attachmentMetadata)
                && s.GetAttachmentContentAsync(attachmentContentId) == Task.FromResult(attachmentContent));

            viewModel = CreateViewModel(questionnaireRepository.Object, interviewRepository.Object, attachmentStorage);
        };

        Because of = () => viewModel.Init("interview", new Identity(entityId, Empty.RosterVector));

        It should_initialize_attachment_as_image = () => viewModel.IsImage.ShouldBeTrue();

        It should_read_image_content = () => viewModel.AttachmentContent.ShouldEqual(attachmentContent);

        static AttachmentViewModel viewModel;
        private static Guid entityId;
        private static byte[] attachmentContent;
    }
}

