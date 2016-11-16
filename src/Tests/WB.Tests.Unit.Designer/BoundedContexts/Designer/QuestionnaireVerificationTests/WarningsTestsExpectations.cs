using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    public static class WarningsTestsExpectations
    {
        public static IReadOnlyCollection<QuestionnaireVerificationMessage> ExpectWarning(this QuestionnaireDocument questionnaire, string warningCode)
            => Create
                .QuestionnaireView(questionnaire)
                .ExpectWarning(warningCode);

        public static IReadOnlyCollection<QuestionnaireVerificationMessage> ExpectWarning(this IEnumerable<SharedPerson> sharedPersons, string warningCode)
            => Create
                .QuestionnaireView(sharedPersons: sharedPersons)
                .ExpectWarning(warningCode);

        public static IReadOnlyCollection<QuestionnaireVerificationMessage> ExpectWarning(this QuestionnaireView questionnaireView, string warningCode)
        {
            // arrange
            var verifier = Create.QuestionnaireVerifier();

            // act
            var messages = verifier.Verify(questionnaireView).ToList();

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
            => Create
                .QuestionnaireView(questionnaire)
                .ExpectNoWarning(warningCode);

        public static IReadOnlyCollection<QuestionnaireVerificationMessage> ExpectNoWarning(this IEnumerable<SharedPerson> sharedPersons, string warningCode)
            => Create
                .QuestionnaireView(sharedPersons: sharedPersons)
                .ExpectNoWarning(warningCode);

        public static IReadOnlyCollection<QuestionnaireVerificationMessage> ExpectNoWarning(this QuestionnaireView questionnaireView, string warningCode)
        {
            // arrange
            var verifier = Create.QuestionnaireVerifier();

            // act
            var messages = verifier.Verify(questionnaireView).ToList();

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