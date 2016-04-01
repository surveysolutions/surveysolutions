using System;
using System.Web.Http;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.UI.Designer.Api.Attributes;
using WB.UI.Designer.Code;
using WB.UI.Shared.Web.Membership;

namespace WB.UI.Designer.Api
{
    [ApiBasicAuth]
    public class ImportController : ImportControllerBase
    {
        private readonly IStringCompressor zipUtils;
        private readonly ISerializer serializer;

        public ImportController(
            IStringCompressor zipUtils,
            IMembershipUserService userHelper,
            IQuestionnaireListViewFactory viewFactory,
            IViewFactory<QuestionnaireViewInputModel, QuestionnaireView> questionnaireViewFactory,
            IViewFactory<QuestionnaireSharedPersonsInputModel, QuestionnaireSharedPersons> sharedPersonsViewFactory,
            IQuestionnaireVerifier questionnaireVerifier,
            IExpressionProcessorGenerator expressionProcessorGenerator,
            IQuestionnaireHelper questionnaireHelper,
            IDesignerEngineVersionService engineVersionService,
            ISerializer serializer)
            : base(userHelper, viewFactory, questionnaireViewFactory, sharedPersonsViewFactory,
                questionnaireVerifier, expressionProcessorGenerator, questionnaireHelper, engineVersionService)
        {
            this.zipUtils = zipUtils;
            this.serializer = serializer;
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

            var questionnaireContentVersion = this.engineVersionService.GetQuestionnaireContentVersion(questionnaireView.Source);

            var resultAssembly = this.GetQuestionnaireAssemblyOrThrow(questionnaireView, questionnaireContentVersion);

            var questionnaire = questionnaireView.Source.Clone();
            questionnaire.Macros = null;
            questionnaire.LookupTables = null;
            questionnaire.SharedPersons = null;
            questionnaire.Attachments = null;

            return new QuestionnaireCommunicationPackage
            {
                Questionnaire = this.zipUtils.CompressString(this.serializer.Serialize(questionnaire, SerializationBinderSettings.NewToOld)), // use binder to serialize to the old namespaces and assembly
                QuestionnaireAssembly = resultAssembly,
                QuestionnaireContentVersion = questionnaireContentVersion.Major
            };
        }
    }
}