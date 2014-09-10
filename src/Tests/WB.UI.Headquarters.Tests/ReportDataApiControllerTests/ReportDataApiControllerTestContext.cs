using Main.Core.View;
using Moq;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.InputModels;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;
using WB.Core.SharedKernels.SurveyManagement.Web.Api;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;

namespace WB.UI.Headquarters.Tests.ReportDataApiControllerTests
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
            IViewFactory<MapReportInputModel, MapReportView> mapReport = null,
            IViewFactory<QuestionnaireBrowseInputModel, QuestionnaireAndVersionsView> questionnaireBrowseViewFactory = null,
            IViewFactory<QuestionnaireQuestionInfoInputModel, QuestionnaireQuestionInfoView> questionInforFactory = null,
            IChartStatisticsViewFactory chartStatisticsViewFactory = null
            )
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
                mapReport ?? Mock.Of<IViewFactory<MapReportInputModel, MapReportView>>(),
                questionnaireBrowseViewFactory ?? Mock.Of<IViewFactory<QuestionnaireBrowseInputModel, QuestionnaireAndVersionsView>>(),
                questionInforFactory ?? Mock.Of<IViewFactory<QuestionnaireQuestionInfoInputModel, QuestionnaireQuestionInfoView>>(),
                chartStatisticsViewFactory ?? Mock.Of<IChartStatisticsViewFactory>()
            );
        }
    }
}