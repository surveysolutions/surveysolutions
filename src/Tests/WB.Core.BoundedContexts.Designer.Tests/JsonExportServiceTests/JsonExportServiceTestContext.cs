using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.Infrastructure.ReadSide;

namespace WB.Core.BoundedContexts.Designer.Tests.JsonExportServiceTests
{
    internal class JsonExportServiceTestContext
    {
        protected static QuestionnaireExportService CreateQuestionnaireExportService(
            IViewFactory<QuestionnaireViewInputModel, QuestionnaireView> questionnaireViewFactory = null,
            IQuestionnaireVersioner versioner = null)
        {
            return new QuestionnaireExportService(versioner ?? Mock.Of<IQuestionnaireVersioner>());
        }

        protected static IViewFactory<QuestionnaireViewInputModel, QuestionnaireView> CreateQuestionnaireViewFactory()
        {
            var questionnaire = new QuestionnaireDocument();
            var questionnaireView = new QuestionnaireView(questionnaire);
            var questionnaireViewFactory =
                Mock.Of<IViewFactory<QuestionnaireViewInputModel, QuestionnaireView>>(
                    x => x.Load(Moq.It.IsAny<QuestionnaireViewInputModel>()) == questionnaireView);
            return questionnaireViewFactory;
        }
    }
}
