using System;
using System.Web.Http;
using Main.Core.Documents;
using Newtonsoft.Json;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.Infrastructure.Native.Storage;
using WB.UI.Designer.Api.Attributes;
using WB.UI.Designer.Code;
using WB.UI.Designer.Services;
using WB.UI.Shared.Web.Membership;

namespace WB.UI.Designer.Api
{
    [ApiBasicAuth]
    public class ImportController : ImportControllerBase
    {
        private readonly IStringCompressor zipUtils;
        private readonly QuestionnaireDowngradeService downgradeService;

        private static readonly JsonSerializerSettings JsonSerializerSettingsNewToOld = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Objects,
            NullValueHandling = NullValueHandling.Ignore,
            FloatParseHandling = FloatParseHandling.Decimal,
            Formatting = Formatting.None,
            Binder = new NewToOldAssemblyRedirectSerializationBinder()
        };

        public ImportController(
            IStringCompressor zipUtils,
            IMembershipUserService userHelper,
            IQuestionnaireListViewFactory viewFactory,
            IQuestionnaireViewFactory questionnaireViewFactory,
            IQuestionnaireSharedPersonsFactory sharedPersonsViewFactory,
            IQuestionnaireVerifier questionnaireVerifier,
            IExpressionProcessorGenerator expressionProcessorGenerator,
            IQuestionnaireHelper questionnaireHelper,
            IDesignerEngineVersionService engineVersionService,
            QuestionnaireDowngradeService downgradeService)
            : base(userHelper, viewFactory, questionnaireViewFactory, sharedPersonsViewFactory,
                questionnaireVerifier, expressionProcessorGenerator, questionnaireHelper, engineVersionService)
        {
            this.zipUtils = zipUtils;
            this.downgradeService = downgradeService;
        }

        [HttpGet]
        public override void Login() 
            => base.Login(); 

        [HttpPost]
        public override PagedQuestionnaireCommunicationPackage PagedQuestionnaireList(QuestionnaireListRequest request)
            => base.PagedQuestionnaireList(request);
        
        [HttpGet]
        public override QuestionnaireListCommunicationPackage QuestionnaireList()
            => base.QuestionnaireList();
        
        [HttpPost]
        public QuestionnaireCommunicationPackage Questionnaire(DownloadQuestionnaireRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var questionnaireView = this.GetQuestionnaireViewOrThrow(request);

            this.CheckInvariantsAndThrowIfInvalid(request, questionnaireView);

            Version questionnaireContentVersion = this.engineVersionService.GetQuestionnaireContentVersion(questionnaireView.Source);

            var resultAssembly = this.GetQuestionnaireAssemblyOrThrow(questionnaireView, questionnaireContentVersion);

            QuestionnaireDocument questionnaire = questionnaireView.Source.Clone();
            questionnaire.Macros = null;
            questionnaire.LookupTables = null;
            questionnaire.SharedPersons = null;
            questionnaire.Attachments = null;
            this.downgradeService.Downgrade(questionnaire, questionnaireContentVersion);

            var serrializedQuestionnaire = JsonConvert.SerializeObject(questionnaire, JsonSerializerSettingsNewToOld);

            return new QuestionnaireCommunicationPackage
            {
                Questionnaire = this.zipUtils.CompressString(serrializedQuestionnaire), 
                QuestionnaireAssembly = resultAssembly,
                QuestionnaireContentVersion = questionnaireContentVersion.Major
            };
        }
    }
}