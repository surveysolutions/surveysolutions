using Moq;
using WB.Core.BoundedContexts.Designer.Implementation.Services.Accounts.Membership;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.UI.Designer.Api.Headquarters;

namespace WB.Tests.Unit.Designer.Applications.ImportControllerTests
{
    internal class ImportControllerTestContext
    {
        protected static ImportV2Controller CreateImportController(
            IQuestionnaireViewFactory questionnaireViewFactory = null,
            IMembershipUserService membershipUserService = null,
            IDesignerEngineVersionService engineVersionService = null,
            IQuestionnaireVerifier questionnaireVerifier = null,
            IExpressionProcessorGenerator expressionProcessorGenerator=null,
            ISerializer serializer=null,
            IStringCompressor zipUtils = null,
            IAttachmentService attachmentService = null)
        {
            return new ImportV2Controller(zipUtils ?? Mock.Of<IStringCompressor>(),
                membershipUserService ?? Mock.Of<IMembershipUserService>(),
                Mock.Of<IQuestionnaireListViewFactory>(),
                questionnaireViewFactory ?? Mock.Of<IQuestionnaireViewFactory>(),
                questionnaireVerifier ?? Mock.Of<IQuestionnaireVerifier>(),
                expressionProcessorGenerator ?? Mock.Of<IExpressionProcessorGenerator>(),
                engineVersionService ?? Mock.Of<IDesignerEngineVersionService>(),
                serializer??Mock.Of<ISerializer>(),
                attachmentService: attachmentService ?? Mock.Of<IAttachmentService>());
        }
    }
}
