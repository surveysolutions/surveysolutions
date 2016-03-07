using System;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Entities.Interview;
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

            var questionnaireMock = Mock.Of<IQuestionnaire>(_
                => _.GetAttachmentIdForEntity(entityId) == null);
            var questionnaireRepository = new Mock<IPlainQuestionnaireRepository>();
            questionnaireRepository.SetReturnsDefault(questionnaireMock);

            var interview = Mock.Of<IStatefulInterview>();
            var interviewRepository = new Mock<IStatefulInterviewRepository>();
            interviewRepository.SetReturnsDefault(interview);

            attachmentStorage = Mock.Of<IQuestionnaireAttachmentStorage>();

            viewModel = CreateViewModel(questionnaireRepository.Object, interviewRepository.Object, attachmentStorage);
        };

        Because of = () => viewModel.Init("interview", new Identity(entityId, Empty.RosterVector));

        It should_dont_call_attachment_storage = () =>
            Mock.Get(attachmentStorage).Verify(s => s.GetAttachmentAsync(Moq.It.IsAny<string>()), Times.Never());

        It should_dont_call_attachment_content = () =>
            Mock.Get(attachmentStorage).Verify(s => s.GetAttachmentContentAsync(Moq.It.IsAny<string>()), Times.Never());

        It should_initialize_image_flag_as_false = () => 
            viewModel.IsImage.ShouldBeFalse();

        It should_be_empty_attachment_content = () => 
            viewModel.AttachmentContent.ShouldBeNull();

        static AttachmentViewModel viewModel;
        private static Guid entityId;
        private static IQuestionnaireAttachmentStorage attachmentStorage;
    }
}

