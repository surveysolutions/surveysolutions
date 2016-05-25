using System;
using System.Web;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Services;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.BoundedContexts.Headquarters.Views.Preloading;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Views.TakeNew;
using WB.Core.BoundedContexts.Headquarters.Views.UsersAndQuestionnaires;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.UI.Headquarters.Controllers;
using WB.UI.Headquarters.Services;
using BatchUploadModel = WB.Core.SharedKernels.SurveyManagement.Web.Models.BatchUploadModel;

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
                () => (sampleImportServiceMock ?? Mock.Of<ISampleImportService>()),
                Mock.Of<IViewFactory<AllUsersAndQuestionnairesInputModel, AllUsersAndQuestionnairesView>>(),
                Mock.Of<IPreloadingTemplateService>(), Mock.Of<IPreloadedDataRepository>(),
                Mock.Of<IPreloadedDataVerifier>(),
                Mock.Of<IViewFactory<SampleUploadViewInputModel, SampleUploadView>>(),
                new InterviewDataExportSettings("", false,10000,100,1,100),
                Mock.Of<IQuestionnaireBrowseViewFactory>(),
                Mock.Of<IInterviewImportService>());
        }
    }
}