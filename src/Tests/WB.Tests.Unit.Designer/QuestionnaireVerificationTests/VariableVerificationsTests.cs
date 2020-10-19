using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Verifier;

namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests
{
    [TestOf(typeof(VariableVerifications))]
    internal class VariableVerificationsTests : QuestionnaireVerifierTestsContext
    {
        [Test]
        public void when_variable_on_cover_page_without_label()
            => QuestionnaireDocumentWithCoverPage(new[]
                {
                    Create.Variable(),
                })
                .ExpectError("WB0310");
    }
}