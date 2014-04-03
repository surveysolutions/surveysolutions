using System;
using System.Web;
using Main.Core.View;
using Moq;
using Ncqrs.Commanding.ServiceModel;
using Questionnaire.Core.Web.Helpers;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire.BrowseItem;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.Survey;
using WB.Core.SharedKernels.SurveyManagement.Views.TakeNew;
using WB.Core.SharedKernels.SurveyManagement.Views.User;
using WB.Core.SharedKernels.SurveyManagement.Views.UsersAndQuestionnaires;
using Web.Supervisor.Controllers;
using Web.Supervisor.Models;

namespace Web.Supervisor.Tests.HQControllerTests
{
    internal class HqControllerTestContext
    {
        protected static QuestionnaireBrowseItem CreateQuestionnaireBrowseItem()
        {
            return new QuestionnaireBrowseItem();
        }

        protected static BatchUploadModel CreateBatchUploadModel(HttpPostedFileBase file, Guid questionnaireId)
        {
            return new BatchUploadModel
            {
                QuestionnaireId = questionnaireId,
                File = file
            };
        }

        protected static HQController CreateHqController(
            IViewFactory<QuestionnaireItemInputModel, QuestionnaireBrowseItem> questionnaireItemFactoryMock = null,
            ISampleImportService sampleImportServiceMock = null)
        {
            return new HQController(
                Mock.Of<ICommandService>(),
                Mock.Of<IGlobalInfoProvider>(),
                Mock.Of<ILogger>(),
                Mock.Of<IViewFactory<QuestionnaireBrowseInputModel, QuestionnaireBrowseView>>(),
                questionnaireItemFactoryMock ?? Mock.Of<IViewFactory<QuestionnaireItemInputModel, QuestionnaireBrowseItem>>(),
                Mock.Of<IViewFactory<UserListViewInputModel, UserListView>>(),
                Mock.Of<IViewFactory<SurveyUsersViewInputModel, SurveyUsersView>>(),
                Mock.Of<IViewFactory<TakeNewInterviewInputModel, TakeNewInterviewView>>(),
                Mock.Of<IViewFactory<UserListViewInputModel, UserListView>>(),
                sampleImportServiceMock ?? Mock.Of<ISampleImportService>(),
                Mock.Of<IViewFactory<AllUsersAndQuestionnairesInputModel, AllUsersAndQuestionnairesView>>());
        }
    }
}