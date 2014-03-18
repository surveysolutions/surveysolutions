using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Designer.Tests.JsonExportServiceTests
{
    internal class JsonExportServiceTestContext
    {
        protected static JsonExportService CreateJsonExportService(
            IReadSideRepositoryReader<QuestionnaireDocument> questionnaireStorage = null,
            IQuestionnaireVersioner versioner = null)
        {
            return new JsonExportService(
                questionnaireStorage ?? Mock.Of<IReadSideRepositoryReader<QuestionnaireDocument>>(),
                versioner ?? Mock.Of<IQuestionnaireVersioner>());
        }
    }
}
