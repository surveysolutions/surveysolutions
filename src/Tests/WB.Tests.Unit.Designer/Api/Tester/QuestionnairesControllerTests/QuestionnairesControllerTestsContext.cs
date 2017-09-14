using Moq;
using NSubstitute;
using WB.Core.BoundedContexts.Designer.Implementation.Services.Accounts.Membership;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.QuestionnaireCompilationForOldVersions;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.UI.Designer.Api.Tester;

namespace WB.Tests.Unit.Designer.Api.Tester.QuestionnairesControllerTests
{
    public class QuestionnairesControllerTestsContext
    {
        public static QuestionnairesController CreateQuestionnairesController(IMembershipUserService userHelper = null,
            IQuestionnaireViewFactory questionnaireViewFactory = null,
            IQuestionnaireVerifier questionnaireVerifier = null,
            IExpressionProcessorGenerator expressionProcessorGenerator = null,
            IQuestionnaireListViewFactory viewFactory = null,
            IDesignerEngineVersionService engineVersionService = null,
            IExpressionsPlayOrderProvider expressionsPlayOrderProvider = null,
            IQuestionnaireCompilationVersionService questionnaireCompilationVersionService = null)
        {
            return new QuestionnairesController(
                userHelper: userHelper ?? Substitute.For<IMembershipUserService>(),
                questionnaireViewFactory: questionnaireViewFactory ?? Substitute.For<IQuestionnaireViewFactory>(),
                questionnaireVerifier: questionnaireVerifier ?? Substitute.For<IQuestionnaireVerifier>(),
                expressionProcessorGenerator: expressionProcessorGenerator ?? Substitute.For<IExpressionProcessorGenerator>(),
                viewFactory: viewFactory ?? Substitute.For<IQuestionnaireListViewFactory>(),
                engineVersionService: engineVersionService ?? Substitute.For<IDesignerEngineVersionService>(),
                expressionsPlayOrderProvider: expressionsPlayOrderProvider ?? Substitute.For<IExpressionsPlayOrderProvider>(),
                
                questionnaireCompilationVersionService: questionnaireCompilationVersionService ?? Mock.Of<IQuestionnaireCompilationVersionService>());
        }
    }
}