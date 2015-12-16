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
        private readonly IInterviewImportService interviewImportService;

        public InterviewsApiController(
            ICommandService commandService, IGlobalInfoProvider globalInfo, ILogger logger,
            IPreloadedDataRepository preloadedDataRepository, Func<ISampleImportService> sampleImportServiceFactory,
            IInterviewImportService interviewImportService)
            : base(commandService, globalInfo, logger)
        {
            this.preloadedDataRepository = preloadedDataRepository;
            this.sampleImportServiceFactory = sampleImportServiceFactory;
            this.interviewImportService = interviewImportService;
        }

        [ObserverNotAllowedApi]
        public void ImportSampleData(BatchUploadModel model)
        {
            PreloadedDataByFile preloadedData = this.preloadedDataRepository.GetPreloadedDataOfSample(model.InterviewId);
            Guid responsibleHeadquarterId = this.GlobalInfo.GetCurrentUser().Id;

            new Task(() =>
            {
                ThreadMarkerManager.MarkCurrentThreadAsIsolated();
                ThreadMarkerManager.MarkCurrentThreadAsNoTransactional();
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
                    ThreadMarkerManager.ReleaseCurrentThreadFromIsolation();
                    ThreadMarkerManager.RemoveCurrentThreadFromNoTransactional();
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
                ThreadMarkerManager.MarkCurrentThreadAsIsolated();
                ThreadMarkerManager.MarkCurrentThreadAsNoTransactional();
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
                    ThreadMarkerManager.ReleaseCurrentThreadFromIsolation();
                    ThreadMarkerManager.RemoveCurrentThreadFromNoTransactional();
                }
            }).Start();
        }

        [HttpGet]
        public InterviewImportStatusApiView GetImportInterviewsStatus()
        {
            var status = this.interviewImportService.Status;
            return new InterviewImportStatusApiView
            {
                QuestionnaireTitle = status.QuestionnaireTitle,
                IsInProgress = status.IsInProgress,
                TotalInterviewsCount = status.TotalInterviewsCount,
                CreatedInterviewsCount = status.CreatedInterviewsCount,
                EstimatedTime = TimeSpan.FromMilliseconds(status.EstimatedTime).ToString(@"dd\.hh\:mm\:ss"),
                ElapsedTime = TimeSpan.FromMilliseconds(status.ElapsedTime).ToString(@"dd\.hh\:mm\:ss")
            };
        }

        public class InterviewImportStatusApiView
        {
            public string QuestionnaireTitle { get; set; }
            public bool IsInProgress { get; set; }
            public int TotalInterviewsCount { get; set; }
            public int CreatedInterviewsCount { get; set; }
            public string ElapsedTime { get; set; }
            public string EstimatedTime { get; set; }
        }
    }
}