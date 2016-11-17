using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    public static class ErrorsTestsExpectations
    {
        public static IReadOnlyCollection<QuestionnaireVerificationMessage> ExpectError(this QuestionnaireDocument questionnaire, string errorCode)
            => Create
                .QuestionnaireView(questionnaire)
                .ExpectError(errorCode);

        public static IReadOnlyCollection<QuestionnaireVerificationMessage> ExpectError(this IEnumerable<SharedPerson> sharedPersons, string errorCode)
            => Create
                .QuestionnaireView(sharedPersons: sharedPersons)
                .ExpectError(errorCode);

        public static IReadOnlyCollection<QuestionnaireVerificationMessage> ExpectError(this QuestionnaireView questionnaireView, string errorCode)
        {
            // arrange
            var verifier = Create.QuestionnaireVerifier();

            // act
            var messages = verifier.Verify(questionnaireView).ToList();

            //assert
            messages.ShouldContainError(errorCode);

            return messages;
        }

        public static IReadOnlyCollection<QuestionnaireVerificationMessage> AndError(this IReadOnlyCollection<QuestionnaireVerificationMessage> messages, string errorCode)
        {
            //assert
            messages.ShouldContainError(errorCode);

            return messages;
        }

        public static IReadOnlyCollection<QuestionnaireVerificationMessage> ExpectNoError(this QuestionnaireDocument questionnaire, string errorCode)
            => Create
                .QuestionnaireView(questionnaire)
                .ExpectNoError(errorCode);

        public static IReadOnlyCollection<QuestionnaireVerificationMessage> ExpectNoError(this IEnumerable<SharedPerson> sharedPersons, string errorCode)
            => Create
                .QuestionnaireView(sharedPersons: sharedPersons)
                .ExpectNoError(errorCode);

        public static IReadOnlyCollection<QuestionnaireVerificationMessage> ExpectNoError(this QuestionnaireView questionnaireView, string errorCode)
        {
            // arrange
            var verifier = Create.QuestionnaireVerifier();

            // act
            var messages = verifier.Verify(questionnaireView).ToList();

            //assert
            messages.ShouldNotContainError(errorCode);

            return messages;
        }

        public static IReadOnlyCollection<QuestionnaireVerificationMessage> AndNoError(this IReadOnlyCollection<QuestionnaireVerificationMessage> messages, string errorCode)
        {
            //assert
            messages.ShouldNotContainError(errorCode);

            return messages;
        }
    }
}