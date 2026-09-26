using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Enumerator.Native.WebInterview;
using WB.Enumerator.Native.WebInterview.Services;
using WB.UI.Headquarters.Controllers;

namespace WB.Tests.Web.Headquarters.Controllers
{
    [TestFixture]
    [TestOf(typeof(WebInterviewBinaryController))]
    public class WebInterviewBinaryControllerTests
    {
        [Test]
        public async Task when_uploading_heic_image_for_cawi_should_store_file_and_answer_question()
        {
            var questionIdentity = WB.Tests.Abc.Create.Entity.Identity(Guid.Parse("22222222-2222-2222-2222-222222222222"));
            var question = WB.Tests.Abc.Create.Entity.InterviewTreeQuestion(questionIdentity, questionType: QuestionType.Multimedia, variableName: "photo");
            var expectedFileName = AnswerUtils.GetPictureFileName(question.VariableName, questionIdentity.RosterVector, ".heic");
            var uploadedBytes = new byte[] { 1, 2, 3, 4, 5 };

            var interview = new Mock<IStatefulInterview>();
            interview.Setup(x => x.Id).Returns(interviewId);
            interview.Setup(x => x.CurrentResponsibleId).Returns(responsibleId);
            interview.Setup(x => x.AcceptsInterviewerAnswers()).Returns(true);
            interview.Setup(x => x.GetQuestion(questionIdentity)).Returns(question);

            var interviewRepository = new Mock<IStatefulInterviewRepository>();
            interviewRepository.Setup(x => x.Get(interviewId.FormatGuid())).Returns(interview.Object);

            var commandService = new Mock<ICommandService>();
            var imageStorage = new Mock<IImageFileStorage>();
            var notificationService = new Mock<IWebInterviewNotificationService>();

            var controller = new WebInterviewBinaryController(
                interviewRepository.Object,
                commandService.Object,
                notificationService.Object,
                Mock.Of<IAudioFileStorage>(),
                Mock.Of<IAudioProcessingService>(),
                imageStorage.Object);

            var file = CreateFormFile(uploadedBytes, "photo.heic", "image/heic");

            var result = await controller.Image(interviewId, questionIdentity.Id.FormatGuid(), file);

            result.Should().BeOfType<JsonResult>()
                .Which.Value.Should().Be("ok");

            imageStorage.Verify(
                x => x.StoreInterviewBinaryData(interviewId, expectedFileName, uploadedBytes, "image/heic"),
                Times.Once);

            commandService.Verify(
                x => x.Execute(It.Is<AnswerPictureQuestionCommand>(c =>
                    c.InterviewId == interviewId
                    && c.UserId == responsibleId
                    && c.QuestionId == questionIdentity.Id
                    && c.PictureFileName == expectedFileName), It.IsAny<string>()),
                Times.Once);

            notificationService.Verify(
                x => x.MarkAnswerAsNotSaved(It.IsAny<Guid>(), It.IsAny<Identity>(), It.IsAny<Exception>()),
                Times.Never);
        }

        private static IFormFile CreateFormFile(byte[] bytes, string fileName, string contentType)
        {
            var stream = new MemoryStream(bytes);
            var file = new FormFile(stream, 0, bytes.Length, "file", fileName)
            {
                Headers = new HeaderDictionary()
            };
            file.ContentType = contentType;
            return file;
        }

        private static readonly Guid interviewId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        private static readonly Guid responsibleId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    }
}
