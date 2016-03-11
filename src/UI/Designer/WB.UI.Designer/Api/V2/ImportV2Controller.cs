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
    [RoutePrefix("api/v2/import")]
    public class ImportV2Controller : ImportControllerBase
    {
        public ImportV2Controller(
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
            : base(
                zipUtils, userHelper, viewFactory, questionnaireViewFactory, sharedPersonsViewFactory,
                questionnaireVerifier, expressionProcessorGenerator, questionnaireHelper, engineVersionService,
                serializer)
        {
        }

        [HttpGet]
        [Route("login")]
        public override void Login() 
            => base.Login(); 

        [HttpPost]
        [Route("PagedQuestionnaireList")]
        public override PagedQuestionnaireCommunicationPackage PagedQuestionnaireList(QuestionnaireListRequest request)
            => base.PagedQuestionnaireList(request);
        

        [HttpGet]
        [Route("QuestionnaireList")]
        public override QuestionnaireListCommunicationPackage QuestionnaireList()
            => base.QuestionnaireList();
        

        [HttpPost]
        [Route("Questionnaire")]
        public QuestionnaireCommunicationPackage Questionnaire(DownloadQuestionnaireRequest request)
            => base.Questionnaire(request, false);
        
    }
}