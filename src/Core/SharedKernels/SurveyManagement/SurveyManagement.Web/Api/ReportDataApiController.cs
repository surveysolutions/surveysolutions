using System;
using System.Linq;
using System.Web.Http;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire.BrowseItem;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.Interviews;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Factories;
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
        private readonly IHeadquartersTeamsAndStatusesReport headquartersTeamsAndStatusesReport;
        private readonly ISupervisorTeamsAndStatusesReport supervisorTeamsAndStatusesReport;

        private readonly IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory;

        private readonly ISurveysAndStatusesReport surveysAndStatusesReport;

        private readonly IChartStatisticsViewFactory chartStatisticsViewFactory;

        private readonly IViewFactory<MapReportInputModel, MapReportView> mapReport;

        private readonly IViewFactory<QuestionnaireQuestionInfoInputModel, QuestionnaireQuestionInfoView> questionInforFactory;
        private readonly IViewFactory<QuantityByInterviewersReportInputModel, QuantityByResponsibleReportView> quantityByInterviewersReport;
        private readonly IViewFactory<QuantityBySupervisorsReportInputModel, QuantityByResponsibleReportView> quantityBySupervisorsReport;

        public ReportDataApiController(
            ICommandService commandService,
            IGlobalInfoProvider provider,
            ILogger logger,
            ISurveysAndStatusesReport surveysAndStatusesReport,
            IHeadquartersTeamsAndStatusesReport headquartersTeamsAndStatusesReport,
            ISupervisorTeamsAndStatusesReport supervisorTeamsAndStatusesReport,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory,
            IViewFactory<MapReportInputModel, MapReportView> mapReport, 
            IViewFactory<QuestionnaireQuestionInfoInputModel, QuestionnaireQuestionInfoView> questionInforFactory,
            IChartStatisticsViewFactory chartStatisticsViewFactory, 
            IViewFactory<QuantityByInterviewersReportInputModel, QuantityByResponsibleReportView> quantityByInterviewersReport, IViewFactory<QuantityBySupervisorsReportInputModel, QuantityByResponsibleReportView> quantityBySupervisorsReport)
            : base(commandService, provider, logger)
        {
            this.surveysAndStatusesReport = surveysAndStatusesReport;
            this.headquartersTeamsAndStatusesReport = headquartersTeamsAndStatusesReport;
            this.supervisorTeamsAndStatusesReport = supervisorTeamsAndStatusesReport;
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
            this.mapReport = mapReport;
            this.questionInforFactory = questionInforFactory;
            this.chartStatisticsViewFactory = chartStatisticsViewFactory;
            this.quantityByInterviewersReport = quantityByInterviewersReport;
            this.quantityBySupervisorsReport = quantityBySupervisorsReport;
        }

        [HttpPost]
        public TeamsAndStatusesReportView SupervisorTeamMembersAndStatusesReport(SummaryListViewModel data)
        {
            var input = new TeamsAndStatusesInputModel
            {
                ViewerId = this.GlobalInfo.GetCurrentUser().Id,
            };


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

            var result = this.supervisorTeamsAndStatusesReport.Load(input);
            return result;
        }

        [HttpPost]
        public MapReportView MapReport(MapReportInputModel data)
        {
            return this.mapReport.Load(data);
        }

        [HttpPost]
        public QuestionnaireAndVersionsView Questionnaires(QuestionnaireBrowseInputModel input)
        {
            QuestionnaireBrowseView questionnaireBrowseView = this.questionnaireBrowseViewFactory.Load(input);
            var result = new QuestionnaireAndVersionsView
            {
                Items = questionnaireBrowseView.Items
                                               .Select(x => new QuestionnaireAndVersionsItem {
                                                        QuestionnaireId = x.QuestionnaireId,
                                                        Title = x.Title,
                                                        Versions = questionnaireBrowseView.Items
                                                                                          .Where(q => q.QuestionnaireId == x.QuestionnaireId)
                                                                                          .Select(y => y.Version)
                                                                                          .ToArray()
                                                    }).ToArray(),
                TotalCount = questionnaireBrowseView.TotalCount
            };
            return result;
        }

        public QuestionnaireQuestionInfoView QuestionInfo(QuestionnaireQuestionInfoInputModel input)
        {
            return this.questionInforFactory.Load(input);
        }

        public QuantityByResponsibleReportView QuantityByInterviewers(QuantityByInterviewersReportModel input)
        {
            if (input.Pager != null)
            {
                input.Request.Page = input.Pager.Page;
                input.Request.PageSize = input.Pager.PageSize;
            }
            if (input.Request.SupervisorId == Guid.Empty)
            {
                input.Request.SupervisorId = this.GlobalInfo.GetCurrentUser().Id;
            }
            return this.quantityByInterviewersReport.Load(input.Request);
        }

        public QuantityByResponsibleReportView QuantityBySupervisors(QuantityBySupervisorsReportModel input)
        {
            if (input.Pager != null)
            {
                input.Request.Page = input.Pager.Page;
                input.Request.PageSize = input.Pager.PageSize;
            }

            return this.quantityBySupervisorsReport.Load(input.Request);
        }

        [HttpPost]
        public SurveysAndStatusesReportView SupervisorSurveysAndStatusesReport(SurveyListViewModel data)
        {
            var input = new SurveysAndStatusesReportInputModel { TeamLeadId = this.GlobalInfo.GetCurrentUser().Id };

            if (data != null)
            {
                input.Orders = data.SortOrder;
                if (data.Pager != null)
                {
                    input.Page = data.Pager.Page;
                    input.PageSize = data.Pager.PageSize;
                }

                if (data.Request != null && data.Request.UserId != input.TeamLeadId)
                {
                    input.ResponsibleId = data.Request.UserId;
                }
            }

            return this.surveysAndStatusesReport.Load(input);
        }

        [HttpPost]
        public TeamsAndStatusesReportView HeadquarterSupervisorsAndStatusesReport(SummaryListViewModel data)
        {
            var input = new TeamsAndStatusesInputModel();

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

            return this.headquartersTeamsAndStatusesReport.Load(input);
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
                    input.TeamLeadId = data.Request.UserId;
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