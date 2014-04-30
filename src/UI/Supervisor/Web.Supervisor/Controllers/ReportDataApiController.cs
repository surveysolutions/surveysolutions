﻿using System.Web.Http;
using Main.Core.View;
using Ncqrs.Commanding.ServiceModel;
using Questionnaire.Core.Web.Helpers;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire.BrowseItem;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.InputModels;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;
using Web.Supervisor.Models;

namespace Web.Supervisor.Controllers
{
    [Authorize(Roles = "Headquarter, Supervisor")]
    public class ReportDataApiController : BaseApiController
    {
        private readonly IViewFactory<HeadquarterSupervisorsAndStatusesReportInputModel, HeadquarterSupervisorsAndStatusesReportView>
            headquarterSupervisorsAndStatusesReport;

        private readonly IViewFactory<HeadquarterSurveysAndStatusesReportInputModel, HeadquarterSurveysAndStatusesReportView>
            headquarterSurveysAndStatusesReport;

        private readonly IViewFactory<SupervisorSurveysAndStatusesReportInputModel, SupervisorSurveysAndStatusesReportView>
            supervisorSurveysAndStatusesReport;

        private readonly IViewFactory<SupervisorTeamMembersAndStatusesReportInputModel, SupervisorTeamMembersAndStatusesReportView>
            supervisorTeamMembersAndStatusesReport;

        private readonly IViewFactory<QuestionnaireBrowseInputModel, QuestionnaireAndVersionsView> questionnaireBrowseViewFactory;

        private readonly IViewFactory<MapReportInputModel, MapReportView> mapReport;

        private readonly IViewFactory<QuestionnaireQuestionInfoInputModel, QuestionnaireQuestionInfoView> questionInforFactory;

        public ReportDataApiController(
            ICommandService commandService,
            IGlobalInfoProvider provider,
            ILogger logger,
            IViewFactory<HeadquarterSurveysAndStatusesReportInputModel, HeadquarterSurveysAndStatusesReportView>
                headquarterSurveysAndStatusesReport,
            IViewFactory<HeadquarterSupervisorsAndStatusesReportInputModel, HeadquarterSupervisorsAndStatusesReportView>
                headquarterSupervisorsAndStatusesReport,
            IViewFactory<SupervisorTeamMembersAndStatusesReportInputModel, SupervisorTeamMembersAndStatusesReportView>
                supervisorTeamMembersAndStatusesReport,
            IViewFactory<SupervisorSurveysAndStatusesReportInputModel, SupervisorSurveysAndStatusesReportView>
                supervisorSurveysAndStatusesReport, IViewFactory<MapReportInputModel, MapReportView> mapReport,
            IViewFactory<QuestionnaireBrowseInputModel, QuestionnaireAndVersionsView> questionnaireBrowseViewFactory, 
            IViewFactory<QuestionnaireQuestionInfoInputModel, QuestionnaireQuestionInfoView> questionInforFactory)
            : base(commandService, provider, logger)
        {
            this.headquarterSurveysAndStatusesReport = headquarterSurveysAndStatusesReport;
            this.headquarterSupervisorsAndStatusesReport = headquarterSupervisorsAndStatusesReport;
            this.supervisorTeamMembersAndStatusesReport = supervisorTeamMembersAndStatusesReport;
            this.supervisorSurveysAndStatusesReport = supervisorSurveysAndStatusesReport;
            this.mapReport = mapReport;
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
            this.questionInforFactory = questionInforFactory;
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
            return mapReport.Load(data);
        }

        [HttpPost]
        public QuestionnaireAndVersionsView Questionnaires(QuestionnaireBrowseInputModel input)
        {
            return questionnaireBrowseViewFactory.Load(input);
        }

        [HttpPost]
        public QuestionnaireQuestionInfoView QuestionInfo(QuestionnaireQuestionInfoInputModel input)
        {
            return questionInforFactory.Load(input);
        }

        [HttpPost]
        public SupervisorSurveysAndStatusesReportView SupervisorSurveysAndStatusesReport(SurveyListViewModel data)
        {
            var input = new SupervisorSurveysAndStatusesReportInputModel(this.GlobalInfo.GetCurrentUser().Id);

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

            return this.supervisorSurveysAndStatusesReport.Load(input);
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
        public HeadquarterSurveysAndStatusesReportView HeadquarterSurveysAndStatusesReport(SurveyListViewModel data)
        {
            var input = new HeadquarterSurveysAndStatusesReportInputModel();

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

            return this.headquarterSurveysAndStatusesReport.Load(input);
        }
    }
}