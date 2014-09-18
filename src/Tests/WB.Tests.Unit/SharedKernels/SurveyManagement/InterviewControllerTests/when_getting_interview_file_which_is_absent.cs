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
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.InterviewControllerTests
{
    internal class when_getting_interview_file_which_is_absent : InterviewControllerTestsContext
    {
        Establish context = () =>
        {
            controller =
                CreateController();
        };

        Because of = () =>
            actionResult = controller.InterviewFile(interviewId, fileName);

        It should_return_file_stream_result = () =>
            actionResult.ShouldBeOfExactType<FileStreamResult>();

        It should_return_file_name_equal_to_no_image_found = () =>
            ((FileStreamResult)actionResult).FileDownloadName.ShouldEqual("no_image_found.jpg");

        private static InterviewController controller;
        private static ActionResult actionResult;
        private static Guid interviewId = Guid.Parse("11111111111111111111111111111111");
        private static string fileName = "file name";
    }
}
