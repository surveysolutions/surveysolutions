using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_question_inside_roster_with_title_which_contains_roster_title_as_substitution_reference : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionId = Guid.Parse("10000000000000000000000000000000");
            questionnaire = CreateQuestionnaireDocument(
                Create.FixedRoster(variable: "a",
                    fixedTitles: new[] {"a", "b"},
                    children: new IComposite[]
                    {
                        new SingleQuestion()
                        {
                            PublicKey = questionId,
                            StataExportCaption = "var",
                            QuestionText = "hello %rostertitle%",
                            Answers =
                            {
                                new Answer() {AnswerValue = "1", AnswerText = "opt 1"},
                                new Answer() {AnswerValue = "2", AnswerText = "opt 2"}
                            }
                        }
                    })
                );

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        It should_return_0_error = () =>
            verificationMessages.Count().ShouldEqual(0);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid questionId;
    }
}
