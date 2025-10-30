using Moq;
using NSubstitute;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.QuestionnaireCompilationForOldVersions;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.UI.Designer.Controllers.Api.Headquarters;

namespace WB.Tests.Unit.Designer.Api.Headquarters.QuestionnairesControllerTests
{
    internal class QuestionnairesControllerTestContext
    {
        protected static HQQuestionnairesController CreateQuestionnairesController(
            IQuestionnaireViewFactory questionnaireViewFactory = null,
            IDesignerEngineVersionService engineVersionService = null,
            IQuestionnaireVerifier questionnaireVerifier = null,
            IExpressionProcessorGenerator expressionProcessorGenerator=null,
            ISerializer serializer=null,
            IStringCompressor zipUtils = null,
            IAttachmentService attachmentService = null,
            IExpressionsPlayOrderProvider expressionsPlayOrderProvider = null,
            IQuestionnaireCompilationVersionService questionnaireCompilationVersionService = null,
            IMacrosSubstitutionService macrosSubstitutionService = null)
        {
            var hqQuestionnairesController = new HQQuestionnairesController(
                questionnaireViewFactory: questionnaireViewFactory ?? Mock.Of<IQuestionnaireViewFactory>(),
                viewFactory: Mock.Of<IQuestionnaireListViewFactory>(),
                questionnaireVerifier: questionnaireVerifier ?? Mock.Of<IQuestionnaireVerifier>(),
                expressionProcessorGenerator: expressionProcessorGenerator ?? Mock.Of<IExpressionProcessorGenerator>(),
                engineVersionService: engineVersionService ?? Mock.Of<IDesignerEngineVersionService>(),
                serializer: serializer ?? Mock.Of<ISerializer>(),
                zipUtils: zipUtils ?? Mock.Of<IStringCompressor>(),
                listItemStorage: Create.InMemoryDbContext(),
                expressionsPlayOrderProvider: expressionsPlayOrderProvider ??
                                              Substitute.For<IExpressionsPlayOrderProvider>(),
                questionnaireCompilationVersionService: questionnaireCompilationVersionService ??
                                                        Mock.Of<IQuestionnaireCompilationVersionService>(),
                questionnaireHistoryVersionsService: Mock.Of<IQuestionnaireHistoryVersionsService>(),
                macrosSubstitutionService: Mock.Of<IMacrosSubstitutionService>());

            return hqQuestionnairesController;
        }
    }
}
