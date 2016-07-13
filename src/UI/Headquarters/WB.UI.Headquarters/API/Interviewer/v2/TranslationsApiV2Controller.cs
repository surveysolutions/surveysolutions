﻿using System.Net;
using System.Net.Http;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Questionnaires.Translations;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Api.Interviewer.v2
{
    [ApiBasicAuth(new[] { UserRoles.Operator })]
    public class TranslationsApiV2Controller : ApiController
    {
        private readonly ITranslationManagementService translations;

        public TranslationsApiV2Controller(ITranslationManagementService translations)
        {
            this.translations = translations;
        }

        [HttpGet]
        [WriteToSyncLog(SynchronizationLogType.GetTranslations)]
        public HttpResponseMessage GetAttachmentContent(string id)
        {
            var questionnaireIdentity = QuestionnaireIdentity.Parse(id);

            var translationInstances = this.translations.GetAll(questionnaireIdentity);

            return Request.CreateResponse(HttpStatusCode.OK, translationInstances);
        }
    }
}