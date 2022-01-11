using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.Controllers.Api.DataCollection.Interviewer.v2
{
    [Authorize(Roles = "Interviewer")]
    [Route("api/interviewer/v2/questionnaires")]
    public class QuestionnairesApiV2Controller : QuestionnairesControllerBase
    {
        public QuestionnairesApiV2Controller(
            IQuestionnaireAssemblyAccessor assemblyAccessor,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory,
            ISerializer serializer,
            IQuestionnaireStorage questionnaireStorage,
            IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireRepository,
            IWebInterviewConfigProvider interviewConfigProvider) : base(
                questionnaireStorage: questionnaireStorage,
                questionnaireRepository: questionnaireRepository,
                assemblyAccessor: assemblyAccessor,
                questionnaireBrowseViewFactory: questionnaireBrowseViewFactory,
                serializer: serializer,
                interviewConfigProvider:interviewConfigProvider)
        {
        }

        [HttpGet]
        [Route("census")]
        [WriteToSyncLog(SynchronizationLogType.GetCensusQuestionnaires)]
        public ActionResult<List<QuestionnaireIdentity>> Census()
        {
            return Ok(new List<QuestionnaireIdentity>());
        }

        [HttpGet]
        [Route("list")]
        public override ActionResult<List<QuestionnaireIdentity>> List() => base.List();

        [HttpGet]
        [Route("{id:guid}/{version:int}/{contentVersion:long}")]
        public override ActionResult<QuestionnaireApiView> Get(Guid id, int version, long contentVersion) => base.Get(id, version, contentVersion);

        [HttpGet]
        [Route("{id:guid}/{version:int}/assembly")]
        public override IActionResult GetAssembly(Guid id, int version) => base.GetAssembly(id, version);

        [HttpPost]
        [Route("{id:guid}/{version:int}/logstate")]
        public override IActionResult LogQuestionnaireAsSuccessfullyHandled(Guid id, int version) => base.LogQuestionnaireAsSuccessfullyHandled(id, version);

        [HttpPost]
        [Route("{id:guid}/{version:int}/assembly/logstate")]
        public override IActionResult LogQuestionnaireAssemblyAsSuccessfullyHandled(Guid id, int version) => base.LogQuestionnaireAssemblyAsSuccessfullyHandled(id, version);

        [HttpGet]
        [Route("{id:guid}/{version:int}/attachments")]
        public override ActionResult<List<string>> GetAttachments(Guid id, int version) => base.GetAttachments(id, version);

        [HttpGet]
        [Route("switchabletoweb")]
        public override ActionResult<List<QuestionnaireIdentity>> SwitchableToWeb() => base.SwitchableToWeb();
    }
}
