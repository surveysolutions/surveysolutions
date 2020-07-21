using Moq;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.AttachmentViewModelTests
{
    [NUnit.Framework.TestOf(typeof(AttachmentViewModel))]
    public class AttachmentViewModelTestContext
    {
        public static AttachmentViewModel CreateViewModel(IQuestionnaireStorage questionnaireRepository = null,
            IStatefulInterviewRepository interviewRepository = null,
            IAttachmentContentStorage attachmentContentStorage = null,
            IEnumeratorSettings enumeratorSettings = null,
            IExternalAppLauncher externalAppLauncher = null)
        {
            return Create.ViewModel.AttachmentViewModel(
                questionnaireRepository ?? Mock.Of<IQuestionnaireStorage>(),
                interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                attachmentContentStorage ?? Mock.Of<IAttachmentContentStorage>());
        }
    }
}
