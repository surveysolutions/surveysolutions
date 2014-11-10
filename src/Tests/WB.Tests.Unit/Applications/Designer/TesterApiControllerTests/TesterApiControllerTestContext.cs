using Main.Core.View;
using Moq;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide;
using WB.UI.Designer.Api;
using WB.UI.Designer.Code;
using WB.UI.Shared.Web.Membership;

namespace WB.Tests.Unit.Applications.Designer.TesterApiControllerTests
{
    internal class TesterApiControllerTestContext
    {
        internal static TesterController CreateQuestionnaireController(
            IMembershipUserService userHelper = null,
            IQuestionnaireHelper questionnaireHelper = null,
            IQuestionnaireVerifier questionnaireVerifier = null,
            IViewFactory<QuestionnaireSharedPersonsInputModel, QuestionnaireSharedPersons> sharedPersonsViewFactory = null,
            IViewFactory<QuestionnaireViewInputModel, QuestionnaireView> questionnaireViewFactory = null,
            IQuestionnaireExportService exportService = null,
            IExpressionProcessorGenerator expressionProcessorGenerator = null,
            ILogger logger = null,
            IArchiveUtils archiver = null)
        {
            return new TesterController(userHelper ?? Mock.Of<IMembershipUserService>(),
                questionnaireHelper ?? Mock.Of<IQuestionnaireHelper>(),
                questionnaireVerifier ?? Mock.Of<IQuestionnaireVerifier>(),
                sharedPersonsViewFactory ?? Mock.Of<IViewFactory<QuestionnaireSharedPersonsInputModel, QuestionnaireSharedPersons>>(),
                questionnaireViewFactory ?? Mock.Of<IViewFactory<QuestionnaireViewInputModel, QuestionnaireView>>(),
                exportService ?? Mock.Of<IQuestionnaireExportService>(),
                expressionProcessorGenerator ?? Mock.Of<IExpressionProcessorGenerator>(),
                logger ?? Mock.Of<ILogger>(), archiver ?? Mock.Of<IArchiveUtils>());
        }
    }
}
