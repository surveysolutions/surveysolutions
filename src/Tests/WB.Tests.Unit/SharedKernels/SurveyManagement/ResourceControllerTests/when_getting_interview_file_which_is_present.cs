using System;
using System.Web.Mvc;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.UI.Headquarters.Controllers;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ResourceControllerTests
{
    internal class when_getting_interview_file_which_is_present : ResourceControllerTestContext
    {
        Establish context = () =>
        {
            controller =
                CreateController(
                    imageFileStorage:
                        Mock.Of<IImageFileStorage>(_ => _.GetInterviewBinaryData(interviewId, fileName) == fileContent));
        };

        Because of = () =>
            actionResult = controller.InterviewFile(interviewId, fileName);

        It should_return_file_content_result = () =>
            actionResult.ShouldBeOfExactType<FileContentResult>();

        It should_return_file_name_equal_to_fileName = () =>
            ((FileContentResult) actionResult).FileDownloadName.ShouldEqual(fileName);

        It should_return_file_content_equal_to_fileContent = () =>
            ((FileContentResult)actionResult).FileContents.ShouldEqual(fileContent);

        private static ResourceController controller;
        private static ActionResult actionResult;
        private static Guid interviewId = Guid.Parse("11111111111111111111111111111111");
        private static string fileName = "file name";
        private static byte[] fileContent = new byte[] { 1 };
    }
}
