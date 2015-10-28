using WB.Core.SharedKernels.Enumerator.Implementation.Services;

namespace WB.Tests.Unit.SharedKernels.Enumerator.Services.QuestionnaireModelBuilderTests
{
    internal class QuestionnaireModelBuilderTestContext
    {
        protected static QuestionnaireModelBuilder CreateQuestionnaireModelBuilder()
        {
            return new QuestionnaireModelBuilder();
        }
    }
}
