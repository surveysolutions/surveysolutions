using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Supervisor.Views.Reposts.InputModels;
using Core.Supervisor.Views.Reposts.Views;
using Main.Core.View;
using Moq;
using Ncqrs.Commanding.ServiceModel;
using Questionnaire.Core.Web.Helpers;
using WB.Core.GenericSubdomains.Logging;
using Web.Supervisor.Controllers;

namespace Web.Supervisor.Tests.ReportDataApiControllerTests
{
    internal class ReportDataApiControllerTestContext
    {
        protected static ReportDataApiController CreateReportDataApiController(
            ICommandService commandService = null,
            IGlobalInfoProvider provider = null,
            ILogger logger = null,
            IViewFactory<HeadquarterSurveysAndStatusesReportInputModel, HeadquarterSurveysAndStatusesReportView>
                headquarterSurveysAndStatusesReport = null,
            IViewFactory<HeadquarterSupervisorsAndStatusesReportInputModel, HeadquarterSupervisorsAndStatusesReportView>
                headquarterSupervisorsAndStatusesReport = null,
            IViewFactory<SupervisorTeamMembersAndStatusesReportInputModel, SupervisorTeamMembersAndStatusesReportView>
                supervisorTeamMembersAndStatusesReport = null,
            IViewFactory<SupervisorSurveysAndStatusesReportInputModel, SupervisorSurveysAndStatusesReportView>
                supervisorSurveysAndStatusesReport = null,
            IViewFactory<MapReportInputModel, MapReportView> mapReport = null)
        {
            return new ReportDataApiController(
                commandService ?? Mock.Of<ICommandService>(),
                provider ?? Mock.Of<IGlobalInfoProvider>(),
                logger ?? Mock.Of<ILogger>(),
                headquarterSurveysAndStatusesReport ??
                    Mock.Of<IViewFactory<HeadquarterSurveysAndStatusesReportInputModel, HeadquarterSurveysAndStatusesReportView>>(),
                headquarterSupervisorsAndStatusesReport ??
                    Mock.Of<IViewFactory<HeadquarterSupervisorsAndStatusesReportInputModel, HeadquarterSupervisorsAndStatusesReportView>>(),
                supervisorTeamMembersAndStatusesReport ??
                    Mock.Of<IViewFactory<SupervisorTeamMembersAndStatusesReportInputModel, SupervisorTeamMembersAndStatusesReportView>>(),
                supervisorSurveysAndStatusesReport ??
                    Mock.Of<IViewFactory<SupervisorSurveysAndStatusesReportInputModel, SupervisorSurveysAndStatusesReportView>>(),
                mapReport ?? Mock.Of<IViewFactory<MapReportInputModel, MapReportView>>());
        }
    }
}
