using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Portable.Implementation.Services;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.AttachmentViewModelTests
{
    [NUnit.Framework.TestOf(typeof(AttachmentViewModel))]

    public class AttachmentViewModelTestContext
    {
        public static AttachmentViewModel CreateViewModel(IQuestionnaireStorage questionnaireRepository = null,
            IStatefulInterviewRepository interviewRepository = null,
            IAttachmentContentStorage attachmentContentStorage = null)
        {
            return new AttachmentViewModel(questionnaireRepository ?? Mock.Of<IQuestionnaireStorage>(),
                interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                attachmentContentStorage ?? Mock.Of<IAttachmentContentStorage>());
        }

    }
}