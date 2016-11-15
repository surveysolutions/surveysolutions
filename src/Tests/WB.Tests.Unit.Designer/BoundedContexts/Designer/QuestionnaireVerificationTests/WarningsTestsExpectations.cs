using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    public static class WarningsTestsExpectations
    {
        public static IReadOnlyCollection<QuestionnaireVerificationMessage> ExpectWarning(this QuestionnaireDocument questionnaire, string warningCode)
        {
            // arrange
            var verifier = Create.QuestionnaireVerifier();

            // act
            var messages = verifier.Verify(Create.QuestionnaireView(questionnaire)).ToList();

            //assert
            messages.ShouldContainWarning(warningCode);

            return messages;
        }

        public static IReadOnlyCollection<QuestionnaireVerificationMessage> AndWarning(this IReadOnlyCollection<QuestionnaireVerificationMessage> messages, string warningCode)
        {
            //assert
            messages.ShouldContainWarning(warningCode);

            return messages;
        }

        public static IReadOnlyCollection<QuestionnaireVerificationMessage> ExpectNoWarning(this QuestionnaireDocument questionnaire, string warningCode)
        {
            // arrange
            var verifier = Create.QuestionnaireVerifier();

            // act
            var messages = verifier.Verify(Create.QuestionnaireView(questionnaire)).ToList();

            //assert
            messages.ShouldNotContainWarning(warningCode);

            return messages;
        }

        public static IReadOnlyCollection<QuestionnaireVerificationMessage> AndNoWarning(this IReadOnlyCollection<QuestionnaireVerificationMessage> messages, string warningCode)
        {
            //assert
            messages.ShouldNotContainWarning(warningCode);

            return messages;
        }
    }
}