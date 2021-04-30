using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.WebApi;

namespace WB.UI.Headquarters.Controllers.Api.DataCollection.Supervisor.v1
{
    [Authorize(Roles = "Supervisor")]
    [Route("api/supervisor/v1")]
    public class QuestionnairesApiV1Controller : QuestionnairesControllerBase
    {
        private readonly IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireRepository;

        public QuestionnairesApiV1Controller(
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
            this.questionnaireRepository = questionnaireRepository;
        }

        [HttpGet]
        [Route("questionnaires/list")]
        public override ActionResult<List<QuestionnaireIdentity>> List() => base.List();

        [HttpGet]
        [Route("questionnaires/{id:guid}/{version:int}/{contentVersion:long}")]
        public override ActionResult<QuestionnaireApiView> Get(Guid id, int version, long contentVersion) => base.Get(id, version, contentVersion);

        [HttpGet]
        [Route("questionnaires/{id:guid}/{version:int}/assembly")]
        public override IActionResult GetAssembly(Guid id, int version) => base.GetAssembly(id, version);

        [HttpPost]
        [Route("questionnaires/{id:guid}/{version:int}/logstate")]
        public override IActionResult LogQuestionnaireAsSuccessfullyHandled(Guid id, int version) => base.LogQuestionnaireAsSuccessfullyHandled(id, version);

        [HttpPost]
        [Route("questionnaires/{id:guid}/{version:int}/assembly/logstate")]
        public override IActionResult LogQuestionnaireAssemblyAsSuccessfullyHandled(Guid id, int version) => base.LogQuestionnaireAssemblyAsSuccessfullyHandled(id, version);

        [HttpGet]
        [Route("questionnaires/{id:guid}/{version:int}/attachments")]
        public override ActionResult<List<string>> GetAttachments(Guid id, int version) => base.GetAttachments(id, version);

        [HttpGet]
        [Route("deletedQuestionnairesList")]
        public List<string> GetDeletedQuestionnaireList()
        {
            var list = questionnaireRepository.Query(_ => _.Where(q => q.IsDeleted == true).ToList())
                .Select(l => l.Id).ToList();
            return list;
        }

        [HttpGet]
        [Route("questionnaires/switchabletoweb")]
        public override ActionResult<List<QuestionnaireIdentity>> SwitchableToWeb() => base.SwitchableToWeb();
    }
}
