using System;
using System.Web.Mvc;
using Machine.Specifications;
using Main.Core.View;
using Moq;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using Web.Supervisor.Controllers;
using Web.Supervisor.Models;
using It = Machine.Specifications.It;

namespace Web.Supervisor.Tests.HQControllerTests
{
    internal class when_hq_controller_gets_empty_file_while_batch_upload_and_model_state_is_invalid : HqControllerTestContext
    {
        Establish context = () =>
        {
            var questionnaireItemFactoryMock = Mock.Of<IViewFactory<QuestionnaireItemInputModel, QuestionnaireBrowseItem>>(x => x.Load(Moq.It.IsAny<QuestionnaireItemInputModel>()) == CreateQuestionnaireBrowseItem());

            inputModel = CreateBatchUploadModel(file: null, questionnaireId: questionnaireId);
            controller = CreateHqController(questionnaireItemFactoryMock: questionnaireItemFactoryMock);

            controller.ViewData.ModelState.Clear();
            controller.ModelState.AddModelError("File", "model is invalid");
        };

        Because of = () =>
        {
            actionResult = controller.BatchUpload(inputModel);
        };

        It should_return_ViewResult = () =>
            actionResult.ShouldBeOfType<ViewResult>();

        It should_return_view_with_name_empty_name = () =>
            ((ViewResult)actionResult).ViewName.ShouldBeEmpty();

        It should_return_view_of_type__BatchUploadModel__ = () =>
            ((ViewResult)actionResult).Model.ShouldBeOfType<BatchUploadModel>();

        It should_return_view_with_model_contains_specified_questionnaire_id = () =>
            (((ViewResult)actionResult).Model as BatchUploadModel).QuestionnaireId.ShouldEqual(questionnaireId);

        private static HQController controller;
        private static BatchUploadModel inputModel;
        private static ActionResult actionResult;
        private static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
    }
}
