using System;
using System.Threading.Tasks;
using Moq;
using MvvmCross.Tests;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels
{
    [TestOf(typeof(MultimediaQuestionViewModel))]
    public class MultimediaQuestionViewModelTests : MvxIoCSupportingTest
    {
        [OneTimeSetUp]
        public void OneTimeSetUp() => base.Setup();

        [Test]
        public async Task when_photo_capture_is_cancelled_should_return_to_picture_question()
        {
            var pictureChooser = new Mock<IPictureChooser>();
            pictureChooser.Setup(x => x.TakePicture()).ThrowsAsync(new TaskCanceledException());
            var userInteractionService = new Mock<IUserInteractionService>();
            userInteractionService
                .Setup(x => x.SelectOneOptionFromList(It.IsAny<string>(), It.IsAny<string[]>()))
                .ReturnsAsync(WB.Core.SharedKernels.Enumerator.Properties.UIResources.Multimedia_TakePhoto);

            var viewModel = new MultimediaQuestionViewModel(
                Mock.Of<IPrincipal>(),
                Mock.Of<IStatefulInterviewRepository>(),
                Mock.Of<IImageFileStorage>(),
                Create.Service.LiteEventRegistry(),
                Mock.Of<IQuestionnaireStorage>(),
                pictureChooser.Object,
                userInteractionService.Object,
                Mock.Of<IViewModelNavigationService>(),
                Create.ViewModel.QuestionState<PictureQuestionAnswered>(),
                Create.ViewModel.QuestionInstructionViewModel(),
                new AnsweringViewModel(Mock.Of<ICommandService>(), Mock.Of<IUserInterfaceStateService>(), Mock.Of<ILogger>()),
                Mock.Of<IFileSystemAccessor>());

            await viewModel.RequestAnswerCommand.ExecuteAsync();

            pictureChooser.Verify(x => x.TakePicture(), Times.Once);
        }
    }
}
