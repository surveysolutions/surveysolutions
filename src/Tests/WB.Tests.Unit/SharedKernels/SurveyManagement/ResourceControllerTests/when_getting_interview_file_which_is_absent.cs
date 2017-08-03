using System;
using System.Web.Mvc;
using FluentAssertions;
using NUnit.Framework;
using WB.UI.Headquarters.Controllers;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ResourceControllerTests
{
    internal class when_getting_interview_file_which_is_absent : ResourceControllerTestContext
    {
        [OneTimeSetUp]
        public void context()
        {
            controller = CreateController();
            Becauseof();
        }

        public void Becauseof() =>
            actionResult = controller.InterviewFile(interviewId, fileName);

        [Test]
        public void should_return_file_stream_result() =>
            actionResult.Should().BeOfType<FileStreamResult>();

        [Test]
        public void should_return_file_name_equal_to_no_image_found() =>
            ((FileStreamResult)actionResult).FileDownloadName.Should().Be("no_image_found.jpg");

        private static ResourceController controller;
        private static ActionResult actionResult;
        private static Guid interviewId = Guid.Parse("11111111111111111111111111111111");
        private static string fileName = "file name";
    }
}
