using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Web.Http;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.Export;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.SurveyStatistics;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.SurveyStatistics.Jobs;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Infrastructure.Native.Monitoring;
using WB.Infrastructure.Native.Sanitizer;
using WB.UI.Headquarters.API.PublicApi.Models;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.API.PublicApi
{
    /// <summary>
    /// Provides a methods for managing report related actions
    /// </summary>
    [ApiBasicAuth(UserRoles.ApiUser, UserRoles.Administrator, TreatPasswordAsPlain = true, FallbackToCookieAuth = true)]
    [RoutePrefix(@"api/v1/report/surveyStatistics")]
    public class SurveyStatisticsReportDataApiController : ApiController
    {
        private readonly ISurveyStatisticsReport surveyStatisticsReport;
        private readonly IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IExportFactory exportFactory;
        private readonly IRefreshReportsTask refreshReportsTask;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IAuthorizedUser authorizedUser;
        private readonly IInterviewReportDataRepository interviewReportDataRepository;

        static readonly Gauge reportQueryTimeGauge = new Gauge(
            @"wb_report_survey_statistics_duration", @"Duration of report variable by team report",
            @"type");

        static readonly Gauge reportQueryCounter = new Gauge(
            @"wb_report_survey_statistics_total", @"Count of report variable by team report",
            @"type");

        public SurveyStatisticsReportDataApiController(ISurveyStatisticsReport surveyStatisticsReport,
            IQuestionnaireStorage questionnaireStorage,
            IFileSystemAccessor fileSystemAccessor,
            IExportFactory exportFactory,
            IRefreshReportsTask refreshReportsTask,
            IAuthorizedUser authorizedUser,
            IInterviewReportDataRepository interviewReportDataRepository,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory)
        {
            this.surveyStatisticsReport = surveyStatisticsReport;
            this.questionnaireStorage = questionnaireStorage;
            this.fileSystemAccessor = fileSystemAccessor;
            this.exportFactory = exportFactory;
            this.refreshReportsTask = refreshReportsTask;
            this.authorizedUser = authorizedUser;
            this.interviewReportDataRepository = interviewReportDataRepository;
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
        }

        [Route(@"questions")]
        public List<QuestionApiView> GetQuestions(string questionnaireId)
        {
            var questionnaireIdentity = QuestionnaireIdentity.Parse(questionnaireId);
            var questionnaire = this.questionnaireStorage.GetQuestionnaireDocument(questionnaireIdentity);

            var questions = this.interviewReportDataRepository
                .QuestionsForQuestionnaireWithData(questionnaireIdentity);

            string[] GetBreadcrumbs(IComposite entity)
            {
                List<IGroup> parents = new List<IGroup>();
                var parent = (IGroup) entity.GetParent();
                while (parent != null && parent != questionnaire)
                {
                    parents.Add(parent);
                    parent = (IGroup) parent.GetParent();
                }

                parents.Reverse();

                return parents.Select(x => x.Title + (x.IsRoster ? @" * " : "")).ToArray();
            }

            return questions
                .Select(id => questionnaire.Find<AbstractQuestion>(id))
                .Select(q => new QuestionApiView
                {
                    Answers = q.Answers.Select(a => new QuestionAnswerView
                    {
                        Answer = (int) a.GetParsedValue(),
                        Text = a.AnswerText,
                        Data = a.AsColumnName()
                    }).ToList(),
                    Breadcrumbs = GetBreadcrumbs(q),
                    PublicKey = q.PublicKey,
                    Type = q.QuestionType.ToString(),
                    StataExportCaption = q.StataExportCaption,
                    Label = q.VariableLabel,
                    HasTotal = q.QuestionType == QuestionType.SingleOption 
                               || q.QuestionType == QuestionType.MultyOption,
                    SupportConditions = q.QuestionType == QuestionType.SingleOption || q.QuestionType == QuestionType.MultyOption,
                    Pivotable = q.QuestionType == QuestionType.SingleOption || q.QuestionType == QuestionType.MultyOption,
                    QuestionText = q.QuestionText.RemoveHtmlTags().Replace(@"%rostertitle%", @"[...]")
                })
                .ToList();
        }

        [Route(@"questionnaires")]
        public List<QuestionnaireApiItem> GetQuestionnairesList()
        {
            var allQuestionnaires = this.questionnaireBrowseViewFactory.Load(new QuestionnaireBrowseInputModel());

            Guid? teamLeadId = null;
            if (this.authorizedUser.IsSupervisor)
            {
                teamLeadId = this.authorizedUser.Id;
            }

            var questionnairesWithData =
                new HashSet<QuestionnaireIdentity>(this.interviewReportDataRepository.QuestionnairesWithData(teamLeadId));

            return allQuestionnaires.Items.Where(q => questionnairesWithData.Contains(q.Identity()))
                .Select(q => new QuestionnaireApiItem(q.QuestionnaireId, q.Version, q.Title, q.LastEntryDate))
                .ToList();
        }

        private HttpResponseMessage ReturnEmptyResult()
        {
            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                data = new string[0],
                recordsTotal = 0,
                recordsFiltered = 0
            }, new JsonMediaTypeFormatter());
        }

        /// <summary>
        /// Generate response of selected type with report for single select question
        /// </summary>
        /// <param name="input">input data</param>
        /// <returns>Report view of proper type</returns>
        [Localizable(false)]
        [HttpGet]
        
        [Route(@"")]
        public HttpResponseMessage Report([FromUri] SurveyStatisticsInput input)
        {
            if (input.QuestionnaireId == null)
            {
                return ReturnEmptyResult();
            }

            var questionnaireIdentity = QuestionnaireIdentity.Parse(input.QuestionnaireId);
            var questionnaire = this.questionnaireStorage.GetQuestionnaireDocument(questionnaireIdentity);

            if (questionnaire == null)
            {
                return ReturnEmptyResult();
            }

            IQuestion GetQuestionByGuidOrStataCaption(string inputVar)
            {
                return Guid.TryParse(inputVar, out Guid entityId)
                    ? questionnaire.Find<IQuestion>(entityId)
                    : questionnaire.Find<IQuestion>(q => q.StataExportCaption == inputVar).FirstOrDefault();
            }

            var question = GetQuestionByGuidOrStataCaption(input.Question);

            if (question != null)
            {
                if (question.LinkedToQuestionId != null
                    || question.LinkedToRosterId != null)
                {
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound,
                        $"Cannot build report for Single Question that linked to roster or other questions"));
                }

                var conditionalQuestion = GetQuestionByGuidOrStataCaption(input.ConditionalQuestion);

                var inputModel = new SurveyStatisticsReportInputModel
                {
                    QuestionnaireIdentity = questionnaireIdentity,
                    Question = question,
                    TeamLeadId = input.TeamLeadId,
                    ShowTeamMembers = input.DetailedView,
                    ShowTeamLead = true,
                    Orders = input.ToOrderRequestItems(),
                    PageSize = input.exportType == null ? input.PageSize : int.MaxValue,
                    Page = input.exportType == null ? input.PageIndex : 1,
                    MinAnswer = input.Min,
                    MaxAnswer = input.Max,
                    Condition = input.Condition,
                    ConditionalQuestion = conditionalQuestion,
                    ExcludeCategories = input.ExcludeCategories,
                    Columns = input.ColummnsList.Select(c => c.Name).ToArray(),
                    Pivot = input.Pivot
                };

                if (this.authorizedUser.IsSupervisor)
                {
                    inputModel.TeamLeadId = this.authorizedUser.Id;
                    inputModel.ShowTeamMembers = true;
                    inputModel.ShowTeamLead = false;
                }

                var stopwatch = Stopwatch.StartNew();

                Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
                var report = this.surveyStatisticsReport.GetReport(inputModel);
                report.Name = $"[ {question.StataExportCaption} ] {question.VariableLabel ?? string.Empty}";

                stopwatch.Stop();
                var reportTimeToMonitor = question.QuestionType + (conditionalQuestion != null ? @"_filtered" : "");
                reportQueryCounter.Labels(reportTimeToMonitor).Inc();
                reportQueryTimeGauge
                    .Labels(reportTimeToMonitor)
                    .Set(stopwatch.Elapsed.TotalSeconds);

                if (input.exportType == null)
                {
                    var response = Request.CreateResponse(HttpStatusCode.OK);
                    var reportJson = report.AsDataTablesJson(input.Draw);
                    response.Content = new StringContent(reportJson.ToString(), Encoding.UTF8, "application/json");
                    return response;
                }

                return CreateReportResponse(input.exportType.Value, report,
                    $"{questionnaire.Title} (ver. {questionnaireIdentity.Version}) {question.StataExportCaption}");
            }

            if (input.EmptyOnError) return ReturnEmptyResult();
            throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound,
                $"Cannot find questionnaire entity with variable name or Id: {input.Question}"));
        }

        [HttpPost]
        [Route("status")]
        public IHttpActionResult GetRefreshStatus()
        {
            var refreshState = this.refreshReportsTask.GetReportState();
            return Json(new
            {
                status = new
                {
                    isRunning = refreshState == RefreshReportsState.Refreshing || refreshState == RefreshReportsState.ScheduledForRefresh,
                    lastRefresh = this.refreshReportsTask.LastRefreshTime()
                }
            });
        }

        [HttpPost]
        [Route("forceRefresh")]
        public HttpResponseMessage ForceRefresh()
        {
            this.refreshReportsTask.ForceRefresh();
            
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        private HttpResponseMessage CreateReportResponse(ExportFileType exportType, ReportView report,
            string reportName)
        {
            var exportFile = this.exportFactory.CreateExportFile(exportType);

            Stream exportFileStream = new MemoryStream(exportFile.GetFileBytes(report));
            var result = new ProgressiveDownload(this.Request).ResultMessage(exportFileStream, exportFile.MimeType);

            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue(@"attachment")
            {
                FileNameStar = $@"{this.fileSystemAccessor.MakeValidFileName(reportName)}{exportFile.FileExtension}"
            };

            return result;
        }
    }
}
