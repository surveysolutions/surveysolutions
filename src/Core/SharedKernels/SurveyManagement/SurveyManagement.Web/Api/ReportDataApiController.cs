using System;
using System.Web.Http;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.Interviews;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.InputModels;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Api  
{
    [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
    public class ReportDataApiController : BaseApiController
    {
        private readonly IViewFactory<HeadquarterSupervisorsAndStatusesReportInputModel, HeadquarterSupervisorsAndStatusesReportView>
            headquarterSupervisorsAndStatusesReport;

        private readonly ISurveysAndStatusesReport
            surveysAndStatusesReport;

        private readonly IChartStatisticsViewFactory chartStatisticsViewFactory;

        private readonly IViewFactory<SupervisorTeamMembersAndStatusesReportInputModel, SupervisorTeamMembersAndStatusesReportView>
            supervisorTeamMembersAndStatusesReport;

        private readonly IViewFactory<QuestionnaireBrowseInputModel, QuestionnaireAndVersionsView> questionnaireBrowseViewFactory;

        private readonly IViewFactory<MapReportInputModel, MapReportView> mapReport;

        private readonly IViewFactory<QuestionnaireQuestionInfoInputModel, QuestionnaireQuestionInfoView> questionInforFactory;

        public ReportDataApiController(
            ICommandService commandService,
            IGlobalInfoProvider provider,
            ILogger logger,
            ISurveysAndStatusesReport
                surveysAndStatusesReport,
            IViewFactory<HeadquarterSupervisorsAndStatusesReportInputModel, HeadquarterSupervisorsAndStatusesReportView>
                headquarterSupervisorsAndStatusesReport,
            IViewFactory<SupervisorTeamMembersAndStatusesReportInputModel, SupervisorTeamMembersAndStatusesReportView>
                supervisorTeamMembersAndStatusesReport,
            IViewFactory<MapReportInputModel, MapReportView> mapReport,
            IViewFactory<QuestionnaireBrowseInputModel, QuestionnaireAndVersionsView> questionnaireBrowseViewFactory, 
            IViewFactory<QuestionnaireQuestionInfoInputModel, QuestionnaireQuestionInfoView> questionInforFactory,
            IChartStatisticsViewFactory chartStatisticsViewFactory)
            : base(commandService, provider, logger)
        {
            this.surveysAndStatusesReport = surveysAndStatusesReport;
            this.headquarterSupervisorsAndStatusesReport = headquarterSupervisorsAndStatusesReport;
            this.supervisorTeamMembersAndStatusesReport = supervisorTeamMembersAndStatusesReport;
            this.mapReport = mapReport;
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
            this.questionInforFactory = questionInforFactory;
            this.chartStatisticsViewFactory = chartStatisticsViewFactory;
        }

        [HttpPost]
        public SupervisorTeamMembersAndStatusesReportView SupervisorTeamMembersAndStatusesReport(SummaryListViewModel data)
        {
            var input = new SupervisorTeamMembersAndStatusesReportInputModel(this.GlobalInfo.GetCurrentUser().Id);

            if (data != null)
            {
                input.Orders = data.SortOrder;
                if (data.Pager != null)
                {
                    input.Page = data.Pager.Page;
                    input.PageSize = data.Pager.PageSize;
                }

                if (data.Request != null)
                {
                    input.TemplateId = data.Request.TemplateId;
                    input.TemplateVersion = data.Request.TemplateVersion;
                }
            }

            return this.supervisorTeamMembersAndStatusesReport.Load(input);
        }

        [HttpPost]
        public MapReportView MapReport(MapReportInputModel data)
        {
            return this.mapReport.Load(data);
        }

        [HttpPost]
        public QuestionnaireAndVersionsView Questionnaires(QuestionnaireBrowseInputModel input)
        {
            return this.questionnaireBrowseViewFactory.Load(input);
        }

        [HttpPost]
        public QuestionnaireQuestionInfoView QuestionInfo(QuestionnaireQuestionInfoInputModel input)
        {
            return this.questionInforFactory.Load(input);
        }

        [HttpPost]
        public SurveysAndStatusesReportView SupervisorSurveysAndStatusesReport(SurveyListViewModel data)
        {
            var input = new SurveysAndStatusesReportInputModel { ViewerId = this.GlobalInfo.GetCurrentUser().Id };

            if (data != null)
            {
                input.Orders = data.SortOrder;
                if (data.Pager != null)
                {
                    input.Page = data.Pager.Page;
                    input.PageSize = data.Pager.PageSize;
                }

                if (data.Request != null)
                {
                    input.UserId = data.Request.UserId;
                }
            }

            return this.surveysAndStatusesReport.Load(input);
        }

        [HttpPost]
        public HeadquarterSupervisorsAndStatusesReportView HeadquarterSupervisorsAndStatusesReport(SummaryListViewModel data)
        {
            var input = new HeadquarterSupervisorsAndStatusesReportInputModel();

            if (data != null)
            {
                input.Orders = data.SortOrder;
                if (data.Pager != null)
                {
                    input.Page = data.Pager.Page;
                    input.PageSize = data.Pager.PageSize;
                }

                if (data.Request != null)
                {
                    input.TemplateId = data.Request.TemplateId;
                    input.TemplateVersion = data.Request.TemplateVersion;
                }
            }

            return this.headquarterSupervisorsAndStatusesReport.Load(input);
        }

        [HttpPost]
        public SurveysAndStatusesReportView HeadquarterSurveysAndStatusesReport(SurveyListViewModel data)
        {
            var input = new SurveysAndStatusesReportInputModel();

            if (data != null)
            {
                input.Orders = data.SortOrder;
                if (data.Pager != null)
                {
                    input.Page = data.Pager.Page;
                    input.PageSize = data.Pager.PageSize;
                }

                if (data.Request != null)
                {
                    input.UserId = data.Request.UserId;
                }
            }

            return this.surveysAndStatusesReport.Load(input);
        }

        [HttpPost]
        public ChartStatisticsView ChartStatistics(InterviewsStatisticsViewModel data)
        {
            var input = new ChartStatisticsInputModel
            {
                QuestionnaireId = data.TemplateId,
                QuestionnaireVersion = data.TemplateVersion,
                CurrentDate = DateTime.Now,
                From = data.From,
                To = data.To
            };

            return this.chartStatisticsViewFactory.Load(input);
        }
    }
}