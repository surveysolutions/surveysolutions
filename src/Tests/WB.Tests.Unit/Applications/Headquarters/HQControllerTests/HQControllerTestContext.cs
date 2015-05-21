using System;
using System.Web;
using Moq;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;
using WB.Core.SharedKernels.SurveyManagement.Views.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.InputModels;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;
using WB.Core.SharedKernels.SurveyManagement.Views.TakeNew;
using WB.Core.SharedKernels.SurveyManagement.Views.User;
using WB.Core.SharedKernels.SurveyManagement.Views.UsersAndQuestionnaires;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.UI.Headquarters.Controllers;

namespace WB.Tests.Unit.Applications.Headquarters.HQControllerTests
{
    internal class HqControllerTestContext
    {
        protected static QuestionnairePreloadingDataItem CreateQuestionnaireBrowseItem()
        {
            return new QuestionnairePreloadingDataItem(Guid.NewGuid(), 1,"", new QuestionDescription[0]);
        }

        protected static BatchUploadModel CreateBatchUploadModel(HttpPostedFileBase file, Guid questionnaireId)
        {
            return new BatchUploadModel
            {
                QuestionnaireId = questionnaireId,
                File = file
            };
        }

        protected static HQController CreateHqController(ISampleImportService sampleImportServiceMock = null)
        {
            return new HQController(
                Mock.Of<ICommandService>(),
                Mock.Of<IGlobalInfoProvider>(),
                Mock.Of<ILogger>(),
                Mock.Of<IViewFactory<TakeNewInterviewInputModel, TakeNewInterviewView>>(),
                Mock.Of<IUserListViewFactory>(),
                () => (sampleImportServiceMock ?? Mock.Of<ISampleImportService>()),
                Mock.Of<IViewFactory<AllUsersAndQuestionnairesInputModel, AllUsersAndQuestionnairesView>>(),
                Mock.Of<IPreloadingTemplateService>(), Mock.Of<IPreloadedDataRepository>(),
                Mock.Of<IPreloadedDataVerifier>(),
                Mock.Of<IViewFactory<QuestionnaireItemInputModel, QuestionnaireBrowseItem>>(),
                new InterviewHistorySettings("", false));
        }
    }
}