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
using WB.Core.SharedKernels.SurveySolutions.Documents;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.AttachmentViewModelTests
{
    internal class when_initializing_entity_without_attachment : AttachmentViewModelTestContext
    {
        Establish context = () =>
        {
            entityId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var questionnaireIdentity = new QuestionnaireIdentity(Guid.NewGuid(), 3);

            var questionnaireRepository = Setup.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireIdentity, _ => true);

            var interview = Mock.Of<IStatefulInterview>(i => i.QuestionnaireIdentity == questionnaireIdentity);
            var interviewRepository = Setup.StatefulInterviewRepository(interview);

            attachmentContentStorage = Mock.Of<IAttachmentContentStorage>();

            viewModel = Create.AttachmentViewModel(questionnaireRepository, interviewRepository, attachmentContentStorage);
        };

        Because of = () => viewModel.InitAsync("interview", new Identity(entityId, Empty.RosterVector)).ConfigureAwait(false);


        It should_dont_call_attachment_content = () =>
            Mock.Get(attachmentContentStorage).Verify(s => s.GetMetadataAsync(Moq.It.IsAny<string>()), Times.Never());

        It should_initialize_image_flag_as_false = () => 
            viewModel.IsImage.ShouldBeFalse();

        It should_be_empty_attachment_content = () => 
            viewModel.Content.ShouldBeNull();


        static AttachmentViewModel viewModel;
        private static Guid entityId;
        private static IAttachmentContentStorage attachmentContentStorage;
    }
}

