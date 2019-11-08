﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Refit;
using WB.Core.SharedKernels.DataCollection.Scenarios;
using WB.Core.SharedKernels.Questionnaire.Api;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;

namespace WB.UI.WebTester.Services
{
    public interface IDesignerWebTesterApi
    {
        [Get("/api/webtester/{token}/info")]
        Task<QuestionnaireLiteInfo> GetQuestionnaireInfoAsync(string token);

        [Get("/api/webtester/{token}/questionnaire")]
        Task<Questionnaire> GetQuestionnaireAsync(string token);

        [Get("/api/webtester/{token}/attachment/{attachmentContentId}")]
        Task<AttachmentContent> GetAttachmentContentAsync(string token, string attachmentContentId);

        [Get("/api/webtester/{token}/translations")]
        Task<List<TranslationDto>> GetTranslationsAsync(string token);

        [Get("/api/webtester/Scenarios/{token}/{scenarioId}")]
        Task<string> GetScenario(string token, int scenarioId);
    }
}
