using NSubstitute;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Overview;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.OverviewViewModelTests
{
    [TestOf(typeof(OverviewViewModel))]
    public class OverviewViewModelTests
    {
        [Test]
        public void when_dispose_should_not_dispose_shared_audio_service()
        {
            var audioService = Substitute.For<IAudioService>();
            var viewModel = new OverviewViewModel(
                Substitute.For<IStatefulInterviewRepository>(),
                Substitute.For<IImageFileStorage>(),
                Substitute.For<IViewModelNavigationService>(),
                audioService,
                Substitute.For<IAudioFileStorage>(),
                Substitute.For<IUserInteractionService>(),
                Substitute.For<IDynamicTextViewModelFactory>(),
                Create.ViewModel.DynamicTextViewModel(),
                Substitute.For<IQuestionnaireStorage>(),
                Substitute.For<IInterviewViewModelFactory>());

            viewModel.Dispose();

            audioService.DidNotReceive().Dispose();
        }
    }
}
