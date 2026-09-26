using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NSubstitute;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels
{
    [TestFixture]
    [TestOf(typeof(MultimediaQuestionViewModel))]
    internal class MultimediaQuestionViewModelTests
    {
        [Test]
        public async Task when_picking_picture_from_gallery_and_post_commit_exception_occurs_should_keep_committed_answer_and_cleanup_old_file()
        {
            var interviewId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var questionIdentity = new Identity(Guid.NewGuid(), RosterVector.Empty);
            const string variableName = "photo";
            const string oldFileName = "photo__.jpg";
            const string newFileName = "photo__.png";
            var oldPicture = new byte[] { 9, 8, 7 };
            var newPicture = new byte[] { 1, 2, 3 };
            DateTime? committedAnswerTimeUtc = null;

            var questionnaireIdentity = Create.Entity.QuestionnaireIdentity();
            var beforeUploadInterview = new Mock<IStatefulInterview>();
            beforeUploadInterview.SetupGet(x => x.Id).Returns(interviewId);
            beforeUploadInterview.Setup(x => x.QuestionnaireIdentity).Returns(questionnaireIdentity);
            beforeUploadInterview.Setup(x => x.Language).Returns((string)null);
            beforeUploadInterview.Setup(x => x.GetMultimediaQuestion(questionIdentity))
                .Returns(new InterviewTreeMultimediaQuestion(oldFileName, DateTime.UtcNow.AddMinutes(-1)));

            var afterCommitInterview = new Mock<IStatefulInterview>();
            afterCommitInterview.SetupGet(x => x.Id).Returns(interviewId);
            afterCommitInterview.Setup(x => x.GetMultimediaQuestion(questionIdentity))
                .Returns(() => new InterviewTreeMultimediaQuestion(newFileName, committedAnswerTimeUtc));

            var interviewRepository = new Mock<IStatefulInterviewRepository>();
            interviewRepository.SetupSequence(x => x.Get(It.IsAny<string>()))
                .Returns(beforeUploadInterview.Object)
                .Returns(afterCommitInterview.Object);

            var questionnaire = Mock.Of<IQuestionnaire>(x =>
                x.GetQuestionVariableName(questionIdentity.Id) == variableName &&
                x.IsSignature(questionIdentity.Id) == false);
            var questionnaireStorage = new Mock<IQuestionnaireStorage>();
            questionnaireStorage
                .Setup(x => x.GetQuestionnaire(questionnaireIdentity, null))
                .Returns(questionnaire);

            var imageFileStorage = new Mock<IImageFileStorage>();
            imageFileStorage.Setup(x => x.GetInterviewBinaryData(interviewId, oldFileName)).Returns(oldPicture);
            imageFileStorage.Setup(x => x.GetInterviewBinaryDataAsync(interviewId, newFileName)).ReturnsAsync(newPicture);

            var pictureChooser = Substitute.For<IPictureChooser>();
            pictureChooser.ChoosePictureGallery()
                .Returns(new ChoosePictureResult("newphoto.png", new MemoryStream(newPicture)));

            var userInteractionService = Substitute.For<IUserInteractionService>();
            userInteractionService
                .SelectOneOptionFromList(UIResources.Multimedia_PictureSource, Arg.Any<string[]>())
                .Returns(UIResources.Multimedia_PickFromGallery);

            var answering = Substitute.For<AnsweringViewModel>();
            answering
                .When(x => x.SendQuestionCommandAndGetResultAsync(Arg.Any<QuestionCommand>()))
                .Do(callInfo => committedAnswerTimeUtc = ((AnswerPictureQuestionCommand)callInfo.Args()[0]).OriginDate.UtcDateTime);
            answering
                .SendQuestionCommandAndGetResultAsync(Arg.Any<QuestionCommand>())
                .Returns(_ => Task.FromException<bool>(new InvalidOperationException("boom")));

            var questionStateViewModel = Substitute.For<QuestionStateViewModel<PictureQuestionAnswered>>();
            var validityViewModel = Substitute.For<ValidityViewModel>();
            questionStateViewModel.Validity.Returns(validityViewModel);

            var fileSystemAccessor = Substitute.For<IFileSystemAccessor>();
            fileSystemAccessor.GetFileExtension("newphoto.png").Returns(".png");

            var viewModel = new MultimediaQuestionViewModel(
                Mock.Of<IPrincipal>(x => x.CurrentUserIdentity == Mock.Of<IUserIdentity>(u => u.UserId == userId)),
                interviewRepository.Object,
                imageFileStorage.Object,
                Substitute.For<IViewModelEventRegistry>(),
                questionnaireStorage.Object,
                pictureChooser,
                userInteractionService,
                Substitute.For<IViewModelNavigationService>(),
                questionStateViewModel,
                Substitute.For<QuestionInstructionViewModel>(),
                answering,
                fileSystemAccessor,
                Substitute.For<ILogger>());

            viewModel.Init(interviewId.ToString("N"), questionIdentity, Create.Other.NavigationState());

            await viewModel.RequestAnswerCommand.ExecuteAsync();

            viewModel.Answer.Should().NotBeNull();
            viewModel.Answer.Should().Equal(newPicture);
            viewModel.AnswerFileName.Should().Be(newFileName);
            await validityViewModel.Received(1).ExecutedWithoutExceptions();
            await validityViewModel.DidNotReceive().ProcessException(Arg.Any<InterviewException>());
            imageFileStorage.Verify(x => x.RemoveInterviewBinaryData(interviewId, oldFileName), Times.Once);
            imageFileStorage.Verify(x => x.RemoveInterviewBinaryData(interviewId, newFileName), Times.Never);
        }
    }
}
