using System;
using System.Web.Mvc;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.UI.Headquarters.Controllers;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ResourceControllerTests
{
    internal class when_getting_interview_file_which_is_present : ResourceControllerTestContext
    {
        [OneTimeSetUp]
        public void context()
        {
            controller =
                CreateController(
                    imageFileStorage:
                        Mock.Of<IImageFileStorage>(_ => _.GetInterviewBinaryData(interviewId, fileName) == fileContent));
            Becauseof();
        }

        public void Becauseof() =>
            actionResult = controller.InterviewFile(interviewId, fileName);

        [Test]
        public void should_return_file_content_result() =>
            actionResult.Should().BeOfType<FileContentResult>();

        [Test]
        public void should_return_file_name_equal_to_fileName() =>
            ((FileContentResult) actionResult).FileDownloadName.Should().Be(fileName);

        [Test]
        public void should_return_file_content_equal_to_fileContent() =>
            ((FileContentResult)actionResult).FileContents.Should().Equal(fileContent);

        private static ResourceController controller;
        private static ActionResult actionResult;
        private static Guid interviewId = Guid.Parse("11111111111111111111111111111111");
        private static string fileName = "file name";
        private static byte[] fileContent = new byte[] { 1 };
    }
}
