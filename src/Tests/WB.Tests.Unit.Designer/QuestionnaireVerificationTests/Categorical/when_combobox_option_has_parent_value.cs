using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests.Categorical
{
    internal class when_combobox_option_has_parent_value : QuestionnaireVerifierTestsContext
    {
        [Test]
        public void should_return_verification_error()
        {
            var questionnaire = CreateQuestionnaireDocumentWithOneChapter(

                Create.SingleOptionQuestion(
                    Id.g1,
                    variable: "var",
                    isComboBox: true,
                    answers:
                    new List<Answer>
                    {
                        new Answer()
                        {
                            AnswerValue = "1",
                            AnswerText = "one",
                            ParentCode = 2,
                            ParentValue = "two1"
                        }
                    })
            );

            var verifier = CreateQuestionnaireVerifier();

            // Act
            var errors = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

            errors.ShouldContainError("WB0280");
        }
    }
}
