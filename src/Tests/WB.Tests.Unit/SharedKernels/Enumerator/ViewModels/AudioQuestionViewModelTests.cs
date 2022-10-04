using System;
using System.Threading;
using Moq;
using MvvmCross.Base;
using MvvmCross.Plugin.Messenger;
using MvvmCross.Tests;
using MvvmCross.Views;
using NSubstitute;
using NUnit.Framework;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels
{
    [TestOf(typeof(AudioQuestionViewModel))]
    internal class AudioQuestionViewModelTests: MvxIoCSupportingTest
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            base.Setup();
            
            var dispatcher = Create.Fake.MvxMainThreadDispatcher1();
            Ioc.RegisterSingleton<IMvxViewDispatcher>(dispatcher);
            Ioc.RegisterSingleton<IMvxMainThreadAsyncDispatcher>(dispatcher);
            Ioc.RegisterType<ThrottlingViewModel>(() => Create.ViewModel.ThrottlingViewModel());
            Ioc.RegisterSingleton<IMvxMessenger>(Mock.Of<IMvxMessenger>());
        }
    
        private static AudioQuestionViewModel CreateAudioQuestionViewModel(IPrincipal principal = null,
            IStatefulInterviewRepository interviewRepository = null,
            IQuestionnaireStorage questionnaireStorage = null,
            QuestionStateViewModel<AudioQuestionAnswered> questionStateViewModel = null,
            AnsweringViewModel answering = null,
            QuestionInstructionViewModel instructionViewModel = null,
            IViewModelEventRegistry liteEventRegistry = null,
            IPermissionsService permissions = null,
            IAudioDialog audioDialog = null,
            IAudioFileStorage audioFileStorage = null,
            IAudioService audioService = null)
        {
            return new AudioQuestionViewModel(
                principal: principal ?? Substitute.For<IPrincipal>(),
                interviewRepository: interviewRepository ?? Substitute.For<IStatefulInterviewRepository>(),
                questionnaireStorage: questionnaireStorage ?? Substitute.For<IQuestionnaireStorage>(),
                questionStateViewModel: questionStateViewModel ?? Substitute.For<QuestionStateViewModel<AudioQuestionAnswered>>(),
                answering: answering ?? Substitute.For<AnsweringViewModel>(),
                instructionViewModel: instructionViewModel ?? Substitute.For<QuestionInstructionViewModel>(),
                liteEventRegistry: liteEventRegistry ?? Substitute.For<IViewModelEventRegistry>(),
                permissions: permissions ?? Substitute.For<IPermissionsService>(),
                audioDialog: audioDialog ?? Substitute.For<IAudioDialog>(),
                audioFileStorage: audioFileStorage ?? Substitute.For<IAudioFileStorage>(),
                audioService: audioService ?? Substitute.For<IAudioService>(),
                Create.Fake.MvxMainThreadAsyncDispatcher());
        }
        [Test]
        public void when_answered_disabled_qustion_should_answer_not_be_saved()
        {
            //arrange
            var mockOfAudioDialog = new Mock<IAudioDialog>();
            mockOfAudioDialog.Setup(x => x.ShowAndStartRecording(Moq.It.IsAny<string>()))
                .Raises(m => m.OnRecorded += null, new EventArgs());

            var mockOfCommandService = new Mock<ICommandService>();
            var answeringViewModel = Create.ViewModel.AnsweringViewModel(commandService: mockOfCommandService.Object);
            var questionStateViewModel = Create.ViewModel.QuestionState<AudioQuestionAnswered>();
            
            var viewModel = CreateAudioQuestionViewModel(audioDialog: mockOfAudioDialog.Object,
                answering: answeringViewModel, questionStateViewModel: questionStateViewModel);
            
            //act
            viewModel.RecordAudioCommand.Execute(null);
            //assert
            mockOfCommandService.Verify(x => x.ExecuteAsync(
                Moq.It.IsAny<AnswerAudioQuestionCommand>(),
                Moq.It.IsAny<string>(),
                Moq.It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
