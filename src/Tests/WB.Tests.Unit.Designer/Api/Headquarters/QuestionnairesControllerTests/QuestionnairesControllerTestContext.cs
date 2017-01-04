using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.UI.Designer.Api.Headquarters;
using WB.UI.Shared.Web.Membership;

namespace WB.Tests.Unit.Designer.Api.Headquarters.QuestionnairesControllerTests
{
    [Subject(typeof(HQQuestionnairesController))]
    internal class QuestionnairesControllerTestContext
    {
        protected static HQQuestionnairesController CreateQuestionnairesController(
            IQuestionnaireViewFactory questionnaireViewFactory = null,
            IMembershipUserService membershipUserService = null,
            IDesignerEngineVersionService engineVersionService = null,
            IQuestionnaireVerifier questionnaireVerifier = null,
            IExpressionProcessorGenerator expressionProcessorGenerator=null,
            ISerializer serializer=null,
            IStringCompressor zipUtils = null,
            IAttachmentService attachmentService = null)
        {
            return new HQQuestionnairesController(
                userHelper: membershipUserService ?? Mock.Of<IMembershipUserService>(),
                questionnaireViewFactory: questionnaireViewFactory ?? Mock.Of<IQuestionnaireViewFactory>(),
                viewFactory: Mock.Of<IQuestionnaireListViewFactory>(),
                questionnaireVerifier: questionnaireVerifier ?? Mock.Of<IQuestionnaireVerifier>(),
                expressionProcessorGenerator: expressionProcessorGenerator??Mock.Of<IExpressionProcessorGenerator>(),
                engineVersionService: engineVersionService ?? Mock.Of<IDesignerEngineVersionService>(),
                serializer: serializer??Mock.Of<ISerializer>(),
                zipUtils: zipUtils ?? Mock.Of<IStringCompressor>(),
                listItemStorage: new TestPlainStorage<QuestionnaireListViewItem>());
        }
    }
}
