using System;
using System.Collections.Generic;
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

        private static IEnumerable<TestCaseData> MissingPermissionCases()
        {
            yield return new TestCaseData(
                    new MissingPermissionsException("no camera", typeof(Permissions.Camera)),
                    UIResources.MissingPermissions_Camera)
                .SetName("when_camera_permission_denied");

            yield return new TestCaseData(
                    new MissingPermissionsException("no storage", typeof(Permissions.StorageWrite)),
                    UIResources.MissingPermissions_Storage)
                .SetName("when_storage_permission_denied");

            yield return new TestCaseData(
                    new MissingPermissionsException("custom permission error", new Exception("inner")),
                    "custom permission error")
                .SetName("when_generic_permission_error_occurs");
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

        private static void SetPrivateFields(MultimediaQuestionViewModel viewModel,
            Identity questionIdentity = null,
            string variableName = "q1")
        {
            var type = typeof(MultimediaQuestionViewModel);
            type.GetField("questionIdentity", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .SetValue(viewModel, questionIdentity ?? Create.Entity.Identity());
            type.GetField("variableName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .SetValue(viewModel, variableName);
        }

        private static MultimediaQuestionViewModel CreateViewModelForPermissionsFailure(
            MissingPermissionsException exception,
            Mock<IUserInteractionService> userInteractionService,
            Mock<ValidityViewModel> validityViewModel)
        {
            var pictureChooser = new Mock<IPictureChooser>();
            pictureChooser.Setup(p => p.TakePicture()).ThrowsAsync(exception);

            userInteractionService
                .Setup(u => u.SelectOneOptionFromList(It.IsAny<string>(), It.IsAny<string[]>()))
                .ReturnsAsync(UIResources.Multimedia_TakePhoto);

            var questionState = new Mock<QuestionStateViewModel<PictureQuestionAnswered>>();
            questionState.Setup(q => q.Validity).Returns(validityViewModel.Object);

            var viewModel = CreateViewModel(
                pictureChooser: pictureChooser.Object,
                userInteractionService: userInteractionService.Object,
                questionStateViewModel: questionState.Object);

            SetPrivateFields(viewModel);
            return viewModel;
        }

        [TestCaseSource(nameof(MissingPermissionCases))]
        public async Task when_permission_request_fails_for_answered_question_should_show_toast(
            MissingPermissionsException exception,
            string expectedMessage)
        {
            var userInteractionService = new Mock<IUserInteractionService>();
            var validityViewModel = new Mock<ValidityViewModel>();
            var viewModel = CreateViewModelForPermissionsFailure(exception, userInteractionService, validityViewModel);
            viewModel.Answer = new byte[] { 1, 2, 3 };

            await viewModel.RequestAnswerCommand.ExecuteAsync();

            userInteractionService.Verify(
                u => u.ShowToast(expectedMessage, It.IsAny<bool>()),
                Times.Once,
                "ShowToast should be called when retake fails but answer already exists");
            validityViewModel.Verify(
                v => v.MarkAnswerAsNotSavedWithMessage(It.IsAny<string>()),
                Times.Never,
                "MarkAnswerAsNotSavedWithMessage should not be called when question already has an answer");
        }

        [TestCaseSource(nameof(MissingPermissionCases))]
        public async Task when_permission_request_fails_for_unanswered_question_should_mark_answer_as_not_saved(
            MissingPermissionsException exception,
            string expectedMessage)
        {
            var userInteractionService = new Mock<IUserInteractionService>();
            var validityViewModel = new Mock<ValidityViewModel>();
            var viewModel = CreateViewModelForPermissionsFailure(exception, userInteractionService, validityViewModel);

            await viewModel.RequestAnswerCommand.ExecuteAsync();

            validityViewModel.Verify(
                v => v.MarkAnswerAsNotSavedWithMessage(expectedMessage),
                Times.Once,
                "MarkAnswerAsNotSavedWithMessage should be called when there is no existing answer");
            userInteractionService.Verify(
                u => u.ShowToast(It.IsAny<string>(), It.IsAny<bool>()),
                Times.Never,
                "ShowToast should not be called when there is no existing answer");
        }
    }
}
