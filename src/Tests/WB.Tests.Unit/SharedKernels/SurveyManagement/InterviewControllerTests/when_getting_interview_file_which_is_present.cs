using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.InterviewControllerTests
{
    internal class when_getting_interview_file_which_is_present : InterviewControllerTestsContext
    {
        Establish context = () =>
        {
            controller =
                CreateController(
                    plainFileRepository:
                        Mock.Of<IPlainFileRepository>(_ => _.GetInterviewBinaryData(interviewId, fileName) == fileContent));
        };

        Because of = () =>
            actionResult = controller.InterviewFile(interviewId, fileName);

        It should_return_file_content_result = () =>
            actionResult.ShouldBeOfExactType<FileContentResult>();

        It should_return_file_name_equal_to_fileName = () =>
            ((FileContentResult) actionResult).FileDownloadName.ShouldEqual(fileName);

        It should_return_file_content_equal_to_fileContent = () =>
            ((FileContentResult)actionResult).FileContents.ShouldEqual(fileContent);

        private static InterviewController controller;
        private static ActionResult actionResult;
        private static Guid interviewId = Guid.Parse("11111111111111111111111111111111");
        private static string fileName = "file name";
        private static byte[] fileContent = new byte[] { 1 };
    }
}
