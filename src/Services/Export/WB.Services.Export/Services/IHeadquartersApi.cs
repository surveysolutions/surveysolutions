﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Refit;
using WB.Services.Export.CsvExport.Exporters;
using WB.Services.Export.Events;
using WB.Services.Export.Interview;
using WB.Services.Export.Interview.Entities;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.Services
{
    /// <summary>
    /// Should not be injected
    /// </summary>
    public interface IHeadquartersApi
    {
        [Get("/api/export/v1/questionnaire/{id}")]
        Task<string> GetQuestionnaireAsync([AliasAs("id")] QuestionnaireId questionnaireId);

        [Get("/api/export/v1/interview/batch/diagnosticsInfo")]
        Task<InterviewDiagnosticsInfo[]> GetInterviewDiagnosticsInfoBatchAsync(
            [Query(CollectionFormat.Multi), AliasAs("id")] Guid[] interviewIds);

        [Get("/api/export/v1/interview/batch/commentaries")]
        Task<List<InterviewComment>> GetInterviewCommentsBatchAsync([Query(CollectionFormat.Multi), AliasAs("id")] Guid[] interviewIds);

        [Get("/api/export/v1/interview/batch/summaries")]
        Task<List<InterviewAction>> GetInterviewSummariesBatchAsync([Query(CollectionFormat.Multi), AliasAs("id")] Guid[] interviewIds);

        [Get("/api/export/v1/interview/{interviewId}/image/{image}")]
        Task<HttpContent> GetInterviewImageAsync(Guid interviewId, string image);
        
        [Get("/api/export/v1/interview/{interviewId}/audio/{audio}")]
        Task<HttpContent> GetInterviewAudioAsync(Guid interviewId, string audio);

        [Get("/api/export/v1/interview/batch/history")]
        Task<List<InterviewHistoryView>> GetInterviewsHistory([Query(CollectionFormat.Multi)] Guid[] id);

        [Get("/api/export/v1/interviews/batch/audioAudit")]
        Task<List<AudioAuditView>> GetAudioAuditInterviewsAsync([Query(CollectionFormat.Multi), AliasAs("id")] Guid[] interviewIds);

        [Get("/api/export/v1/interview/{interviewId}/audioAudit/{audio}")]
        Task<HttpContent> GetAudioAuditAsync(Guid interviewId, string audio);

        [Get("/api/export/v1/questionnaire/{id}/audioAudit")]
        Task<QuestionnaireAudioAuditView> DoesSupportAudioAuditAsync([AliasAs("id")] QuestionnaireId questionnaireId);

        [Get("/api/export/v1/interview/events")]
        Task<EventsFeed> GetInterviewEvents([AliasAs("sequence")] long sequence, int pageSize = 500);
    }
}
