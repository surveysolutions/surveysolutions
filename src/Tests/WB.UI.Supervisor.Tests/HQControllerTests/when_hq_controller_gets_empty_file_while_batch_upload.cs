using System;
using System.Web.Mvc;
using Machine.Specifications;
using Main.Core.View;
using Moq;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Supervisor.Controllers;
using It = Machine.Specifications.It;

namespace WB.UI.Supervisor.Tests.HQControllerTests
{
    internal class when_hq_controller_gets_empty_file_while_batch_upload_and_model_state_is_invalid : HqControllerTestContext
    {
        Establish context = () =>
        {
            inputModel = CreateBatchUploadModel(file: null, questionnaireId: questionnaireId);
            controller = CreateHqController();

            controller.ViewData.ModelState.Clear();
            controller.ModelState.AddModelError("File", "model is invalid");
        };

        Because of = () =>
        {
            actionResult = controller.SampleBatchUpload(inputModel);
        };

        It should_return_ViewResult = () =>
            actionResult.ShouldBeOfExactType<ViewResult>();

        It should_return_view_with_name_BatchUpload = () =>
            ((ViewResult)actionResult).ViewName.ShouldEqual("BatchUpload");

        It should_return_view_of_type__BatchUploadModel__ = () =>
            ((ViewResult)actionResult).Model.ShouldBeOfExactType<BatchUploadModel>();

        It should_return_view_with_model_contains_specified_questionnaire_id = () =>
            (((ViewResult)actionResult).Model as BatchUploadModel).QuestionnaireId.ShouldEqual(questionnaireId);

        private static HQController controller;
        private static BatchUploadModel inputModel;
        private static ActionResult actionResult;
        private static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
    }
}