using System;
using System.Threading.Tasks;
using System.Web.Http;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.Storage;
using WB.Core.SharedKernels.SurveyManagement.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.PreloadedData;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.Core.SharedKernels.SurveyManagement.Web.Models.Api;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize(Roles = "Administrator, Headquarter")]
    [ApiValidationAntiForgeryToken]
    public class InterviewsApiController : BaseApiController
    {
        private readonly IPreloadedDataRepository preloadedDataRepository;
        readonly Func<ISampleImportService> sampleImportServiceFactory;

        public InterviewsApiController(
            ICommandService commandService, IGlobalInfoProvider globalInfo, ILogger logger,
            IPreloadedDataRepository preloadedDataRepository, Func<ISampleImportService> sampleImportServiceFactory)
            : base(commandService, globalInfo, logger)
        {
            this.preloadedDataRepository = preloadedDataRepository;
            this.sampleImportServiceFactory = sampleImportServiceFactory;
        }

        [ObserverNotAllowedApi]
        public void ImportSampleData(BatchUploadModel model)
        {
            PreloadedDataByFile preloadedData = this.preloadedDataRepository.GetPreloadedDataOfSample(model.InterviewId);
            Guid responsibleHeadquarterId = this.GlobalInfo.GetCurrentUser().Id;

            new Task(() =>
            {
                IsolatedThreadManager.MarkCurrentThreadAsIsolated();

                try
                {
                    var sampleImportService = this.sampleImportServiceFactory.Invoke();

                    sampleImportService.CreateSample(
                        model.QuestionnaireId,
                        model.QuestionnaireVersion,
                        model.InterviewId,
                        preloadedData,
                        responsibleHeadquarterId,
                        model.ResponsibleSupervisor);
                }
                finally
                {
                    IsolatedThreadManager.ReleaseCurrentThreadFromIsolation();
                }
            }).Start();
        }

        [ObserverNotAllowedApi]
        public void ImportPanelData(BatchUploadModel model)
        {
            PreloadedDataByFile[] preloadedData = this.preloadedDataRepository.GetPreloadedDataOfPanel(model.InterviewId);
            Guid responsibleHeadquarterId = this.GlobalInfo.GetCurrentUser().Id;

            new Task(() =>
            {
                IsolatedThreadManager.MarkCurrentThreadAsIsolated();

                try
                {
                    var sampleImportService = this.sampleImportServiceFactory.Invoke();

                    sampleImportService.CreatePanel(
                        model.QuestionnaireId,
                        model.QuestionnaireVersion,
                        model.InterviewId,
                        preloadedData,
                        responsibleHeadquarterId,
                        model.ResponsibleSupervisor);
                }
                finally
                {
                    IsolatedThreadManager.ReleaseCurrentThreadFromIsolation();
                }
            }).Start();
        }
    }
}