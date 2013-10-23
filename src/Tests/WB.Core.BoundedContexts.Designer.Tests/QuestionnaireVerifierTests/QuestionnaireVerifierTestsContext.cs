using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Implementation.Services;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireVerifierTests
{
    [Subject(typeof(QuestionnaireVerifier))]
    internal class QuestionnaireVerifierTestsContext
    {
        protected static QuestionnaireVerifier CreateQuestionnaireVerifier()
        {
            return new QuestionnaireVerifier();
        }

        protected static QuestionnaireDocument CreateQuestionnaireDocument()
        {
            return new QuestionnaireDocument();
        }
    }
}