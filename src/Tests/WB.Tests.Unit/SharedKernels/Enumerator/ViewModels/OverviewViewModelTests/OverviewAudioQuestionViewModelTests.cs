using NSubstitute;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Overview;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.OverviewViewModelTests
{
    [TestOf(typeof(OverviewAudioQuestionViewModel))]
    public class OverviewAudioQuestionViewModelTests
    {
        [Test]
        public void when_dispose_should_not_dispose_shared_audio_service()
        {
            var audioService = Substitute.For<IAudioService>();
            var questionIdentity = Create.Entity.Identity();
            var treeQuestion = Create.Entity.InterviewTreeQuestion(questionIdentity);
            var section = Create.Entity.InterviewTreeSection(children: treeQuestion);

            Create.Entity.InterviewTree(sections: section);

            var viewModel = new OverviewAudioQuestionViewModel(
                treeQuestion,
                Substitute.For<IAudioFileStorage>(),
                audioService,
                Substitute.For<IUserInteractionService>(),
                Substitute.For<IStatefulInterview>());

            viewModel.Dispose();

            audioService.DidNotReceive().Dispose();
        }
    }
}
