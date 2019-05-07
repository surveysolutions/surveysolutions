using Moq;
using NSubstitute;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.QuestionnaireCompilationForOldVersions;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.UI.Designer.Controllers.Api.Tester;

namespace WB.Tests.Unit.Designer.Api.Tester.QuestionnairesControllerTests
{
    public class QuestionnairesControllerTestsContext
    {
        public static QuestionnairesController CreateQuestionnairesController(
            IQuestionnaireViewFactory questionnaireViewFactory = null,
            IQuestionnaireVerifier questionnaireVerifier = null,
            IExpressionProcessorGenerator expressionProcessorGenerator = null,
            IQuestionnaireListViewFactory viewFactory = null,
            IDesignerEngineVersionService engineVersionService = null,
            IExpressionsPlayOrderProvider expressionsPlayOrderProvider = null,
            IQuestionnaireCompilationVersionService questionnaireCompilationVersionService = null)
        {
            return new QuestionnairesController(
                questionnaireViewFactory: questionnaireViewFactory ?? Substitute.For<IQuestionnaireViewFactory>(),
                questionnaireVerifier: questionnaireVerifier ?? Substitute.For<IQuestionnaireVerifier>(),
                expressionProcessorGenerator: expressionProcessorGenerator ?? Substitute.For<IExpressionProcessorGenerator>(),
                viewFactory: viewFactory ?? Substitute.For<IQuestionnaireListViewFactory>(),
                engineVersionService: engineVersionService ?? Substitute.For<IDesignerEngineVersionService>(),
                expressionsPlayOrderProvider: expressionsPlayOrderProvider ?? Substitute.For<IExpressionsPlayOrderProvider>(),
                questionnaireCompilationVersionService: questionnaireCompilationVersionService ?? Mock.Of<IQuestionnaireCompilationVersionService>(),
                serializer: Mock.Of<ISerializer>());
        }
    }
}
