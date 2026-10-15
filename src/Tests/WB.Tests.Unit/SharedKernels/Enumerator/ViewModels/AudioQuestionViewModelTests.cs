using System;
using System.Threading;
using FluentAssertions;
using Moq;
using MvvmCross.Base;
using MvvmCross.Plugin.Messenger;
using MvvmCross.Tests;
using MvvmCross.Views;
using NSubstitute;
using NUnit.Framework;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
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

        [Test]
        public void when_dispose_should_not_dispose_shared_audio_service()
        {
            var audioService = Substitute.For<IAudioService>();

            var viewModel = CreateAudioQuestionViewModel(audioService: audioService);

            viewModel.Dispose();

            audioService.DidNotReceive().Dispose();
        }

        [Test]
        public void when_init_with_answered_question_should_can_be_played_use_stored_filename()
        {
            // arrange
            var interviewId = Guid.NewGuid();
            var questionIdentity = Create.Entity.Identity();
            const string storedFileName = "old_variable__.m4a";

            var audioQuestion = new InterviewTreeAudioQuestion(storedFileName, TimeSpan.FromSeconds(8));

            var questionnaireIdentity = Create.Entity.QuestionnaireIdentity();
            var interviewMock = new Mock<IStatefulInterview>();
            interviewMock.Setup(x => x.Id).Returns(interviewId);
            interviewMock.Setup(x => x.QuestionnaireIdentity).Returns(questionnaireIdentity);
            interviewMock.Setup(x => x.Language).Returns((string)null);
            interviewMock.Setup(x => x.GetAudioQuestion(questionIdentity)).Returns(audioQuestion);

            var questionnaire = Mock.Of<IQuestionnaire>(x =>
                x.GetQuestionVariableName(questionIdentity.Id) == "new_variable");

            var questionnaireStorageMock = new Mock<IQuestionnaireStorage>();
            questionnaireStorageMock
                .Setup(x => x.GetQuestionnaire(It.IsAny<QuestionnaireIdentity>(), It.IsAny<string>()))
                .Returns(questionnaire);

            var interviewRepositoryMock = new Mock<IStatefulInterviewRepository>();
            interviewRepositoryMock.Setup(x => x.Get(It.IsAny<string>())).Returns(interviewMock.Object);

            var audioFileStorageMock = new Mock<IAudioFileStorage>();
            audioFileStorageMock
                .Setup(x => x.GetInterviewBinaryData(interviewId, storedFileName))
                .Returns(new byte[] { 1, 2, 3 });

            var audioService = Substitute.For<IAudioService>();
            audioService.GetAudioType().Returns("m4a");

            var viewModel = CreateAudioQuestionViewModel(
                interviewRepository: interviewRepositoryMock.Object,
                questionnaireStorage: questionnaireStorageMock.Object,
                audioFileStorage: audioFileStorageMock.Object,
                audioService: audioService);

            // act
            viewModel.Init(interviewId.ToString("N"), questionIdentity, Create.Other.NavigationState());

            // assert
            viewModel.CanBePlayed.Should().BeTrue();
        }

        [Test]
        public void when_answers_removed_event_handled_should_set_can_be_played_to_false()
        {
            // arrange
            var interviewId = Guid.NewGuid();
            var questionIdentity = Create.Entity.Identity();
            const string storedFileName = "stored_variable__.m4a";

            var audioQuestion = new InterviewTreeAudioQuestion(storedFileName, TimeSpan.FromSeconds(5));

            var questionnaireIdentity = Create.Entity.QuestionnaireIdentity();
            var interviewMock = new Mock<IStatefulInterview>();
            interviewMock.Setup(x => x.Id).Returns(interviewId);
            interviewMock.Setup(x => x.QuestionnaireIdentity).Returns(questionnaireIdentity);
            interviewMock.Setup(x => x.Language).Returns((string)null);
            interviewMock.Setup(x => x.GetAudioQuestion(questionIdentity)).Returns(audioQuestion);

            var questionnaire = Mock.Of<IQuestionnaire>(x =>
                x.GetQuestionVariableName(questionIdentity.Id) == "stored_variable");

            var questionnaireStorageMock = new Mock<IQuestionnaireStorage>();
            questionnaireStorageMock
                .Setup(x => x.GetQuestionnaire(It.IsAny<QuestionnaireIdentity>(), It.IsAny<string>()))
                .Returns(questionnaire);

            var interviewRepositoryMock = new Mock<IStatefulInterviewRepository>();
            interviewRepositoryMock.Setup(x => x.Get(It.IsAny<string>())).Returns(interviewMock.Object);

            var audioFileStorageMock = new Mock<IAudioFileStorage>();
            audioFileStorageMock
                .Setup(x => x.GetInterviewBinaryData(interviewId, storedFileName))
                .Returns(new byte[] { 1, 2, 3 });

            var audioService = Substitute.For<IAudioService>();
            audioService.GetAudioType().Returns("m4a");

            var viewModel = CreateAudioQuestionViewModel(
                interviewRepository: interviewRepositoryMock.Object,
                questionnaireStorage: questionnaireStorageMock.Object,
                audioFileStorage: audioFileStorageMock.Object,
                audioService: audioService);

            viewModel.Init(interviewId.ToString("N"), questionIdentity, Create.Other.NavigationState());

            // act
            var @event = new AnswersRemoved(null, new[] { questionIdentity }, DateTimeOffset.UtcNow);
            viewModel.Handle(@event);

            // assert
            viewModel.CanBePlayed.Should().BeFalse();
        }
    }
}
