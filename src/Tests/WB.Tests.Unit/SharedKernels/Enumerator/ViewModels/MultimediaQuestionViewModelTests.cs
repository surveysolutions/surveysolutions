using System;
using System.Reflection;
using System.Threading.Tasks;
using Moq;
using MvvmCross.Base;
using MvvmCross.Plugin.Messenger;
using MvvmCross.Tests;
using MvvmCross.Views;
using NUnit.Framework;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.Tests.Abc;
using Xamarin.Essentials;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels
{
    [TestOf(typeof(MultimediaQuestionViewModel))]
    internal class MultimediaQuestionViewModelTests : MvxIoCSupportingTest
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

        private static MultimediaQuestionViewModel CreateViewModel(
            IPrincipal principal = null,
            IStatefulInterviewRepository interviewRepository = null,
            IImageFileStorage imageFileStorage = null,
            IViewModelEventRegistry eventRegistry = null,
            IQuestionnaireStorage questionnaireStorage = null,
            IPictureChooser pictureChooser = null,
            IUserInteractionService userInteractionService = null,
            IViewModelNavigationService viewModelNavigationService = null,
            QuestionStateViewModel<PictureQuestionAnswered> questionStateViewModel = null,
            QuestionInstructionViewModel instructionViewModel = null,
            AnsweringViewModel answering = null,
            IFileSystemAccessor fileSystemAccessor = null)
        {
            return new MultimediaQuestionViewModel(
                principal: principal ?? Mock.Of<IPrincipal>(p => p.CurrentUserIdentity == Mock.Of<IUserIdentity>(u => u.UserId == Guid.NewGuid())),
                interviewRepository: interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                imageFileStorage: imageFileStorage ?? Mock.Of<IImageFileStorage>(),
                eventRegistry: eventRegistry ?? Mock.Of<IViewModelEventRegistry>(),
                questionnaireStorage: questionnaireStorage ?? Mock.Of<IQuestionnaireStorage>(),
                pictureChooser: pictureChooser ?? Mock.Of<IPictureChooser>(),
                userInteractionService: userInteractionService ?? Mock.Of<IUserInteractionService>(),
                viewModelNavigationService: viewModelNavigationService ?? Mock.Of<IViewModelNavigationService>(),
                questionStateViewModel: questionStateViewModel ?? Mock.Of<QuestionStateViewModel<PictureQuestionAnswered>>(),
                instructionViewModel: instructionViewModel ?? Mock.Of<QuestionInstructionViewModel>(),
                answering: answering ?? Mock.Of<AnsweringViewModel>(),
                fileSystemAccessor: fileSystemAccessor ?? Mock.Of<IFileSystemAccessor>());
        }

        // Calls the private RequestAnswerAsync method directly, bypassing MvvmCross command infrastructure.
        private static Task InvokeRequestAnswerAsync(MultimediaQuestionViewModel viewModel)
        {
            var method = typeof(MultimediaQuestionViewModel)
                .GetMethod("RequestAnswerAsync", BindingFlags.NonPublic | BindingFlags.Instance);
            return (Task)method!.Invoke(viewModel, null);
        }

        // Sets private fields that would normally be initialized by Init(), allowing tests
        // to call RequestAnswerAsync without a fully wired interview/questionnaire.
        private static void SetPrivateFields(MultimediaQuestionViewModel viewModel,
            Identity questionIdentity = null,
            string variableName = "q1")
        {
            var type = typeof(MultimediaQuestionViewModel);
            type.GetField("questionIdentity", BindingFlags.NonPublic | BindingFlags.Instance)!
                .SetValue(viewModel, questionIdentity ?? Create.Entity.Identity());
            type.GetField("variableName", BindingFlags.NonPublic | BindingFlags.Instance)!
                .SetValue(viewModel, variableName);
        }

        [Test]
        public async Task when_camera_permission_denied_and_question_already_has_answer_should_not_mark_question_invalid()
        {
            // arrange
            var pictureChooser = new Mock<IPictureChooser>();
            pictureChooser.Setup(p => p.TakePicture())
                .ThrowsAsync(new MissingPermissionsException("no camera", typeof(Permissions.Camera)));

            var userInteractionService = new Mock<IUserInteractionService>();
            userInteractionService.Setup(u => u.SelectOneOptionFromList(It.IsAny<string>(), It.IsAny<string[]>()))
                .ReturnsAsync(UIResources.Multimedia_TakePhoto);

            var validityViewModel = new Mock<ValidityViewModel>();

            var questionState = new Mock<QuestionStateViewModel<PictureQuestionAnswered>>();
            questionState.Setup(q => q.Validity).Returns(validityViewModel.Object);

            var viewModel = CreateViewModel(
                pictureChooser: pictureChooser.Object,
                userInteractionService: userInteractionService.Object,
                questionStateViewModel: questionState.Object);

            // simulate question already has an answer
            viewModel.Answer = new byte[] { 1, 2, 3 };
            SetPrivateFields(viewModel);

            // act — call private method directly to avoid MvvmCross thread-dispatch infrastructure
            await InvokeRequestAnswerAsync(viewModel);

            // assert: validity should NOT be marked with an error
            validityViewModel.Verify(
                v => v.MarkAnswerAsNotSavedWithMessage(It.IsAny<string>()),
                Times.Never,
                "MarkAnswerAsNotSavedWithMessage should not be called when question already has an answer");
        }

        [Test]
        public async Task when_camera_permission_denied_and_question_already_has_answer_should_show_toast()
        {
            // arrange
            var pictureChooser = new Mock<IPictureChooser>();
            pictureChooser.Setup(p => p.TakePicture())
                .ThrowsAsync(new MissingPermissionsException("no camera", typeof(Permissions.Camera)));

            var userInteractionService = new Mock<IUserInteractionService>();
            userInteractionService.Setup(u => u.SelectOneOptionFromList(It.IsAny<string>(), It.IsAny<string[]>()))
                .ReturnsAsync(UIResources.Multimedia_TakePhoto);

            var validityViewModel = new Mock<ValidityViewModel>();
            var questionState = new Mock<QuestionStateViewModel<PictureQuestionAnswered>>();
            questionState.Setup(q => q.Validity).Returns(validityViewModel.Object);

            var viewModel = CreateViewModel(
                pictureChooser: pictureChooser.Object,
                userInteractionService: userInteractionService.Object,
                questionStateViewModel: questionState.Object);

            viewModel.Answer = new byte[] { 1, 2, 3 };
            SetPrivateFields(viewModel);

            // act
            await InvokeRequestAnswerAsync(viewModel);

            // assert: a toast should be shown
            userInteractionService.Verify(
                u => u.ShowToast(It.IsAny<string>(), It.IsAny<bool>()),
                Times.Once,
                "ShowToast should be called when retake fails but answer already exists");
        }

        [Test]
        public async Task when_camera_permission_denied_and_question_has_no_answer_should_mark_question_invalid()
        {
            // arrange
            var pictureChooser = new Mock<IPictureChooser>();
            pictureChooser.Setup(p => p.TakePicture())
                .ThrowsAsync(new MissingPermissionsException("no camera", typeof(Permissions.Camera)));

            var userInteractionService = new Mock<IUserInteractionService>();
            userInteractionService.Setup(u => u.SelectOneOptionFromList(It.IsAny<string>(), It.IsAny<string[]>()))
                .ReturnsAsync(UIResources.Multimedia_TakePhoto);

            var validityViewModel = new Mock<ValidityViewModel>();
            var questionState = new Mock<QuestionStateViewModel<PictureQuestionAnswered>>();
            questionState.Setup(q => q.Validity).Returns(validityViewModel.Object);

            var viewModel = CreateViewModel(
                pictureChooser: pictureChooser.Object,
                userInteractionService: userInteractionService.Object,
                questionStateViewModel: questionState.Object);

            // Answer is null (no existing answer)
            SetPrivateFields(viewModel);

            // act
            await InvokeRequestAnswerAsync(viewModel);

            // assert: validity should be marked with an error
            validityViewModel.Verify(
                v => v.MarkAnswerAsNotSavedWithMessage(It.IsAny<string>()),
                Times.Once,
                "MarkAnswerAsNotSavedWithMessage should be called when there is no existing answer");
        }
    }
}
