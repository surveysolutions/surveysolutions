using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.Implementation.ReadSide;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Schedulers.InterviewDetailsDataScheduler;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Models;
using WB.UI.Shared.Web.Filters;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Api
{
    [LocalOrDevelopmentAccessOnly]
    public class ControlPanelApiController : ApiController
    {
        private readonly InterviewDetailsDataProcessorContext interviewDetailsDataProcessorContext;
        private readonly IReadSideAdministrationService readSideAdministrationService;

        public ControlPanelApiController(InterviewDetailsDataProcessorContext interviewDetailsDataProcessorContext,
            IReadSideAdministrationService readSideAdministrationService)
        {
            this.interviewDetailsDataProcessorContext = interviewDetailsDataProcessorContext;
            this.readSideAdministrationService = readSideAdministrationService;
        }

        public InterviewDetailsSchedulerViewModel InterviewDetails()
        {
            return new InterviewDetailsSchedulerViewModel()
            {
                Messages = this.interviewDetailsDataProcessorContext.GetMessages().ToArray()
            };
        }

        public IEnumerable<ReadSideEventHandlerDescription> GetAllAvailableHandlers()
        {
            return this.readSideAdministrationService.GetAllAvailableHandlers();
        }

        public ReadSideStatus GetReadSideStatus()
        {
            return this.readSideAdministrationService.GetRebuildStatus();
        }

        [HttpPost]
        public void RebuildReadSide(RebuildReadSideInputViewModel model)
        {
            switch (model.RebuildType)
            {
                case RebuildReadSideType.All:
                    this.readSideAdministrationService.RebuildAllViewsAsync(model.NumberOfSkipedEvents);
                    break;
                case RebuildReadSideType.ByHandlers:
                    this.readSideAdministrationService.RebuildViewsAsync(model.ListOfHandlers, model.NumberOfSkipedEvents);
                    break;
                case RebuildReadSideType.ByHandlersAndEventSource:
                    this.readSideAdministrationService.RebuildViewForEventSourcesAsync(model.ListOfHandlers, model.ListOfEventSources);
                    break;
            }
        }

        [HttpPost]
        public void StopReadSideRebuilding()
        {
            this.readSideAdministrationService.StopAllViewsRebuilding();
        }
    }
}