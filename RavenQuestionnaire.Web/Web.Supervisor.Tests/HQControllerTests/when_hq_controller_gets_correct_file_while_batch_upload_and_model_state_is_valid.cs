using System;
using System.Web;
using System.Web.Mvc;
using Machine.Specifications;
using Main.Core.View;
using Moq;
using WB.Core.BoundedContexts.Supervisor.Implementation;
using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using Web.Supervisor.Controllers;
using Web.Supervisor.Models;
using It = Machine.Specifications.It;

namespace Web.Supervisor.Tests.HQControllerTests
{
    internal class when_hq_controller_gets_correct_file_while_batch_upload_and_model_state_is_valid : HqControllerTestContext
    {
        Establish context = () =>
        {
            var questionnaireItemFactoryMock = Mock.Of<IViewFactory<QuestionnaireItemInputModel, QuestionnaireBrowseItem>>(x => x.Load(Moq.It.IsAny<QuestionnaireItemInputModel>()) == CreateQuestionnaireBrowseItem());

            var sampleImportServiceMock = Mock.Of<ISampleImportService>(x => x.ImportSampleAsync(Moq.It.IsAny<Guid>(), Moq.It.IsAny<ISampleRecordsAccessor>()) == Guid.Parse("10000000000000000000000000000000"));
            inputModel = CreateBatchUploadModel(file: Mock.Of<HttpPostedFileBase>(), questionnaireId: questionnaireId);
            controller = CreateHqController(questionnaireItemFactoryMock: questionnaireItemFactoryMock, sampleImportServiceMock: sampleImportServiceMock);
        };

        Because of = () =>
        {
            actionResult = controller.BatchUpload(inputModel);
        };

        It should_return_ViewResult = () =>
            actionResult.ShouldBeOfType<ViewResult>();

        It should_return_view_with_name_empty_name = () =>
            ((ViewResult)actionResult).ViewName.ShouldEqual("ImportSample");

        It should_return_view_of_type__QuestionnaireBrowseItem__ = () =>
            ((ViewResult)actionResult).Model.ShouldBeOfType<QuestionnaireBrowseItem>();

        private static HQController controller;
        private static BatchUploadModel inputModel;
        private static ActionResult actionResult;
        private static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
    }
}