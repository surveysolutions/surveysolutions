﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.DataExport;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.Export;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.SurveyStatistics;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.SurveyStatistics.Data;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Infrastructure.Native.Monitoring;
using WB.Infrastructure.Native.Sanitizer;
using WB.UI.Headquarters.API;
using WB.UI.Headquarters.API.PublicApi.Models;
using WB.UI.Headquarters.API.PublicApi.Models.Statistics;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi
{
    /// <summary>
    /// Provides a methods for managing report related actions
    /// </summary>
    [Authorize(Roles = "ApiUser, Administrator, Supervisor, Headquarter")]
    [Route(@"api/v1/statistics")]
    [PublicApiJson]
    public class StatisticsController : ControllerBase
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
        /// <param name="questionnaireId">Questionnaire Id</param>
        /// <param name="version">Questionnaire version</param>
        /// <returns>List of questions</returns>
        [Route(@"questions")]
        [HttpGet]
        public List<QuestionDto> GetQuestions(string questionnaireId, long? version = null)
        {
            var questionsList = this.interviewReportDataRepository
                .QuestionsForQuestionnaireWithData(questionnaireId, version);
            
            string[] GetBreadcrumbs(IComposite entity, QuestionnaireDocument questionnaire)
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

            var parsedQuestionList = questionsList
                .Select<QuestionnaireItem, (Guid QuestionId, QuestionnaireIdentity identity)>(
                    q => (q.QuestionId, QuestionnaireIdentity.Parse(q.QuestionnaireIdentity)));

            IEnumerable<(Guid questionId, QuestionnaireIdentity identity)> sortedList = 
                from items in parsedQuestionList
                group items by (items.identity.QuestionnaireId, items.QuestionId)
                into g
                select g.OrderByDescending(o => o.identity.Version).First();

            return sortedList
                .Select<(Guid questionId, QuestionnaireIdentity identity), (IQuestion q, QuestionnaireDocument doc)>(item =>
                {
                    var doc = this.questionnaireStorage.GetQuestionnaireDocument(item.identity);
                    return (doc.Find<IQuestion>(item.questionId), doc);
                })
                .Select(item => new QuestionDto
                {
                    Answers = item.q.Answers.Select(a => new QuestionAnswerView
                    {
                        Answer = (int)a.GetParsedValue(),
                        Text = a.AnswerText,
                        Column = a.AsColumnName()
                    }).ToList(),
                    Breadcrumbs = GetBreadcrumbs(item.q, item.doc),
                    Id = item.q.PublicKey,
                    Type = item.q.QuestionType.ToString(),
                    VariableName = item.q.StataExportCaption,
                    Label = item.q.VariableLabel,
                    HasTotal = item.q.QuestionType == QuestionType.SingleOption
                               || item.q.QuestionType == QuestionType.MultyOption,
                    SupportConditions = item.q.QuestionType == QuestionType.SingleOption 
                                        || item.q.QuestionType == QuestionType.MultyOption,
                    QuestionText = item.q.QuestionText.RemoveHtmlTags().Replace(@"%rostertitle%", @"[...]")
                })
                .ToList();
        }

        [Route(@"questionnaires")]
        [HttpGet]
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

            var questionnairesDtos = allQuestionnaires.Items.Where(q => questionnairesWithData.Contains(q.Identity()))
                .OrderByDescending(q => q.LastEntryDate)
                .Select(q => new QuestionnaireDto
                {
                    Id = q.QuestionnaireId.FormatGuid(),
                    Title = q.Title,
                    Version = q.Version
                }).ToList();
            return questionnairesDtos;
        }

        private ActionResult ReturnEmptyResult(SurveyStatisticsQuery query)
        {
            if (!query.exportType.HasValue)
            {
                return Ok(new
                {
                    data = new string[0],
                    recordsTotal = 0,
                    recordsFiltered = 0
                });
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
        public ActionResult Report(SurveyStatisticsQuery query)
        {
            if (query.QuestionnaireId == null)
            {
                return ReturnEmptyResult(query);
            }

            QuestionnaireDocument questionnaire;

            if (query.Version == null)
            {
                var allQuestionnaires = this.questionnaireBrowseViewFactory.GetAllQuestionnaireIdentities()
                    .Where(q => q.QuestionnaireId.FormatGuid() == query.QuestionnaireId)
                    .ToList();

                var questionnaireId = allQuestionnaires.OrderByDescending(q => q.Version).First();

                questionnaire = this.questionnaireStorage.GetQuestionnaireDocument(questionnaireId);
            }
            else
            {
                questionnaire =
                    this.questionnaireStorage.GetQuestionnaireDocument(Guid.Parse(query.QuestionnaireId),
                        query.Version.Value);

            }

            IQuestion GetQuestionByGuidOrStataCaption(string inputVar)
            {
                return Guid.TryParse(inputVar, out Guid entityId)
                    ? questionnaire.Find<IQuestion>(entityId)
                    : questionnaire.Find<IQuestion>(q => q.StataExportCaption == inputVar).FirstOrDefault();
            }

            if (string.IsNullOrWhiteSpace(query.Question))
                return ReturnEmptyResult(query);

            var question = GetQuestionByGuidOrStataCaption(query.Question);

            if (question == null) return ReturnEmptyResult(query);

            if (question.LinkedToQuestionId != null
                || question.LinkedToRosterId != null)
            {
                return NotFound(
                    $"Cannot build report for Single Question that linked to roster or other questions");
            }

            var conditionalQuestion = GetQuestionByGuidOrStataCaption(query.ConditionalQuestion);

            var inputModel = new SurveyStatisticsReportInputModel
            {
                QuestionnaireId = query.QuestionnaireId,
                QuestionnaireVersion = query.Version,
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
                Orders = query.ToOrderRequestItems(),
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
                var reportJson = report.AsDataTablesJson(query.Draw);
                return Content(reportJson.ToString(), "application/json", Encoding.UTF8);
            }

            return CreateReportResponse(query.exportType.Value, report,
                $"{questionnaire.Title} (ver. {query.Version}) {question.StataExportCaption}");

        }      

        private ActionResult CreateReportResponse(
            ExportFileType exportType, ReportView report, string reportName)
        {
            var exportFile = this.exportFactory.CreateExportFile(exportType);

            Stream exportFileStream = new MemoryStream(exportFile.GetFileBytes(report));

            var file = File(exportFileStream, exportFile.MimeType,
                $"{this.fileSystemAccessor.MakeValidFileName(reportName)}{exportFile.FileExtension}");
            return file;
        }
    }
}
