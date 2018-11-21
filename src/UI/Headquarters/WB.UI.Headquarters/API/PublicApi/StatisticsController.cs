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
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Infrastructure.Native.Monitoring;
using WB.Infrastructure.Native.Sanitizer;
using WB.UI.Headquarters.API.PublicApi.Models;
using WB.UI.Headquarters.API.PublicApi.Models.Statistics;
using WB.UI.Headquarters.Code;
using WB.UI.Shared.Web.Extensions;

namespace WB.UI.Headquarters.API.PublicApi
{
    /// <summary>
    /// Provides a methods for managing report related actions
    /// </summary>
    [ApiBasicAuth(UserRoles.ApiUser, UserRoles.Administrator, TreatPasswordAsPlain = true, FallbackToCookieAuth = true)]
    [RoutePrefix(@"api/v1/statistics")]
    public class StatisticsController : ApiController
    {
        private readonly ISurveyStatisticsReport surveyStatisticsReport;
        private readonly IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IExportFactory exportFactory;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IAuthorizedUser authorizedUser;
        private readonly IInterviewReportDataRepository interviewReportDataRepository;

        static readonly Gauge reportQueryTimeGauge = new Gauge(
            @"wb_report_survey_statistics_duration", @"Duration of report variable by team report",
            @"type");

        static readonly Gauge reportQueryCounter = new Gauge(
            @"wb_report_survey_statistics_total", @"Count of report variable by team report",
            @"type");

        public StatisticsController(ISurveyStatisticsReport surveyStatisticsReport,
            IQuestionnaireStorage questionnaireStorage,
            IFileSystemAccessor fileSystemAccessor,
            IExportFactory exportFactory,
            IAuthorizedUser authorizedUser,
            IInterviewReportDataRepository interviewReportDataRepository,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory)
        {
            this.surveyStatisticsReport = surveyStatisticsReport;
            this.questionnaireStorage = questionnaireStorage;
            this.fileSystemAccessor = fileSystemAccessor;
            this.exportFactory = exportFactory;
            this.authorizedUser = authorizedUser;
            this.interviewReportDataRepository = interviewReportDataRepository;
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
        }

        /// <summary>
        /// Get questions list
        /// </summary>
        /// <remarks>
        /// Gets a questions list for specified questionnaire identity. Only questions that have data are shown
        /// </remarks>
        /// <param name="questionnaireId">Questionnaire Identity</param>
        /// <returns>List of questions</returns>
        [Route(@"questions")]
        public List<QuestionDto> GetQuestions(string questionnaireId)
        {
            var questionnaireIdentity = QuestionnaireIdentity.Parse(questionnaireId);
            var questionnaire = this.questionnaireStorage.GetQuestionnaireDocument(questionnaireIdentity);

            var questions = this.interviewReportDataRepository
                .QuestionsForQuestionnaireWithData(questionnaireIdentity);

            string[] GetBreadcrumbs(IComposite entity)
            {
                List<IGroup> parents = new List<IGroup>();
                var parent = (IGroup)entity.GetParent();
                while (parent != null && parent != questionnaire)
                {
                    parents.Add(parent);
                    parent = (IGroup)parent.GetParent();
                }

                parents.Reverse();

                return parents.Select(x => x.Title + (x.IsRoster ? @" * " : "")).ToArray();
            }

            return questions
                .Select(id => questionnaire.Find<IQuestion>(id))
                .Select(q => new QuestionDto
                {
                    Answers = q.Answers.Select(a => new QuestionAnswerView
                    {
                        Answer = (int)a.GetParsedValue(),
                        Text = a.AnswerText,
                        Column = a.AsColumnName()
                    }).ToList(),
                    Breadcrumbs = GetBreadcrumbs(q),
                    Id = q.PublicKey,
                    Type = q.QuestionType.ToString(),
                    VariableName = q.StataExportCaption,
                    Label = q.VariableLabel,
                    HasTotal = q.QuestionType == QuestionType.SingleOption
                               || q.QuestionType == QuestionType.MultyOption,
                    SupportConditions = q.QuestionType == QuestionType.SingleOption || q.QuestionType == QuestionType.MultyOption,
                    QuestionText = q.QuestionText.RemoveHtmlTags().Replace(@"%rostertitle%", @"[...]")
                })
                .ToList();
        }

        [Route(@"questionnaires")]
        public List<QuestionnaireDto> GetQuestionnairesList()
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
                .OrderByDescending(q => q.LastEntryDate)
                .Select(q => new QuestionnaireDto
                {
                    Identity = q.Id,
                    Title = q.Title,
                    Version = q.Version
                }).ToList();
        }

        private HttpResponseMessage ReturnEmptyResult(SurveyStatisticsQuery query)
        {
            if (!query.exportType.HasValue)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    data = new string[0],
                    recordsTotal = 0,
                    recordsFiltered = 0
                }, new JsonMediaTypeFormatter());
            }
            else
            {
                return CreateReportResponse(query.exportType.Value, new ReportView()
                {
                    Columns = new string[0],
                    Data = new object[0][],
                    Headers = new string[0],
                    Name = "report",
                    TotalCount = 0,
                    Totals = new object[0]
                }, "survey-statistics");
            }
        }

        /// <summary>
        /// Generate report report based on provided query paramters.
        /// </summary>
        /// <param name="query">input data</param>
        /// <returns>Report view of selected type (JSON by default)</returns>
        [Localizable(false)]
        [HttpGet]
        [Route(@"")]
        public HttpResponseMessage Report([FromUri] SurveyStatisticsQuery query)
        {
            if (query.QuestionnaireId == null)
            {
                return ReturnEmptyResult(query);
            }

            var questionnaireIdentity = QuestionnaireIdentity.Parse(query.QuestionnaireId);
            var questionnaire = this.questionnaireStorage.GetQuestionnaireDocument(questionnaireIdentity);

            if (questionnaire == null)
            {
                return ReturnEmptyResult(query);
            }

            IQuestion GetQuestionByGuidOrStataCaption(string inputVar)
            {
                return Guid.TryParse(inputVar, out Guid entityId)
                    ? questionnaire.Find<IQuestion>(entityId)
                    : questionnaire.Find<IQuestion>(q => q.StataExportCaption == inputVar).FirstOrDefault();
            }

            var question = GetQuestionByGuidOrStataCaption(query.Question);

            if (question != null)
            {
                if (question.LinkedToQuestionId != null
                    || question.LinkedToRosterId != null)
                {
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound,
                        $"Cannot build report for Single Question that linked to roster or other questions"));
                }

                var conditionalQuestion = GetQuestionByGuidOrStataCaption(query.ConditionalQuestion);

                var inputModel = new SurveyStatisticsReportInputModel
                {
                    QuestionnaireIdentity = questionnaireIdentity,
                    Question = question,
                    ShowTeamMembers = query.ExpandTeams,
                    PageSize = query.exportType == null ? query.PageSize : int.MaxValue,
                    Page = query.exportType == null ? query.PageIndex : 1,
                    MinAnswer = query.Min,
                    MaxAnswer = query.Max,
                    Condition = query.Condition,
                    ConditionalQuestion = conditionalQuestion,
                    Columns = query?.ColummnsList?.Select(c => c.Name)?.ToArray(),
                    Pivot = query.Pivot,
                    Orders = query.ToOrderRequestItems()
                };

                if (this.authorizedUser.IsSupervisor)
                {
                    inputModel.TeamLeadId = this.authorizedUser.Id;
                    inputModel.ShowTeamMembers = true;
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

                if (query.exportType == null)
                {
                    var response = Request.CreateResponse(HttpStatusCode.OK);
                    var reportJson = report.AsDataTablesJson(query.Draw);
                    response.Content = new StringContent(reportJson.ToString(), Encoding.UTF8, "application/json");
                    return response;
                }

                return CreateReportResponse(query.exportType.Value, report,
                    $"{questionnaire.Title} (ver. {questionnaireIdentity.Version}) {question.StataExportCaption}");
            }

            return ReturnEmptyResult(query);
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
