using System;
using System.Web;
using Core.Supervisor.Views.Survey;
using Core.Supervisor.Views.TakeNew;
using Core.Supervisor.Views.User;
using Core.Supervisor.Views.UsersAndQuestionnaires;
using Main.Core.View;
using Moq;
using Ncqrs.Commanding.ServiceModel;
using Questionnaire.Core.Web.Helpers;
using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire.BrowseItem;
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