using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_questionniare_has_long_enablement_condition : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            StringBuilder enablementCondition = new StringBuilder(201);
            for (int i = 0; i < 201; i++)
            {
                enablementCondition.Append(i);
            }
            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                Create.Question(
                    questionId: questionId,
                    enablementCondition: enablementCondition.ToString()));

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () => errors = verifier.Verify(questionnaire);

        It should_produce_WB0205_warning = () => errors.ShouldContainWarning("WB0209");

        It should_reference_wrong_question = () => 
            errors.Single(x => x.Code == "WB0209").References.ShouldContainOnly(Create.VerificationReference(id: questionId));

        static Guid questionId;
        static QuestionnaireVerifier verifier;
        static QuestionnaireDocument questionnaire;
        static IEnumerable<QuestionnaireVerificationMessage> errors;
    }
}