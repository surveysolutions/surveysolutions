using NSubstitute;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.UI.Designer.Api.Tester;
using WB.UI.Shared.Web.Membership;

namespace WB.Tests.Unit.Designer.Api.Tester.QuestionnairesControllerTests
{
    public class QuestionnairesControllerTestsContext
    {
        public static QuestionnairesController CreateQuestionnairesController(IMembershipUserService userHelper = null,
            IQuestionnaireViewFactory questionnaireViewFactory = null,
            IQuestionnaireVerifier questionnaireVerifier = null,
            IExpressionProcessorGenerator expressionProcessorGenerator = null,
            IQuestionnaireListViewFactory viewFactory = null,
            IDesignerEngineVersionService engineVersionService = null)
        {
            return new QuestionnairesController(
                userHelper: userHelper ?? Substitute.For<IMembershipUserService>(),
                questionnaireViewFactory: questionnaireViewFactory ?? Substitute.For<IQuestionnaireViewFactory>(),
                questionnaireVerifier: questionnaireVerifier ?? Substitute.For<IQuestionnaireVerifier>(),
                expressionProcessorGenerator: expressionProcessorGenerator ?? Substitute.For<IExpressionProcessorGenerator>(),
                viewFactory: viewFactory ?? Substitute.For<IQuestionnaireListViewFactory>(),
                engineVersionService: engineVersionService ?? Substitute.For<IDesignerEngineVersionService>());
        }
    }
}