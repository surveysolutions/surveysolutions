using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_question_that_reference_in_substitutions_variable_from_deeper_level : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionnaire = Create.QuestionnaireDocument(children: Create.Chapter(children: new IComposite[]
            {
                Create.TextQuestion(questionId, text: "Var: %variable1%"),
                Create.FixedRoster(rosterId, fixedTitles: new[] {"1", "2", "3"}, children: new List<IComposite>
                {
                    Create.Variable(variableId, VariableType.LongInteger, "variable1")
                })
            }));

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        It should_return_message_with_code__WB0019 = () =>
            verificationMessages.ShouldContainError("WB0019");

        It should_return_WB0019_error_with_2_references_on_questions = () =>
            verificationMessages.GetError("WB0019")
                .References.Select(x => x.Type)
                .ShouldContainOnly(QuestionnaireVerificationReferenceType.Question, QuestionnaireVerificationReferenceType.Variable);

        It should_return_WB0019_error_with_first_reference_to_question_with_substitution_text = () =>
            verificationMessages.GetError("WB0019").References.ElementAt(0).Id.ShouldEqual(questionId);

        It should_return_WB0019_error_with_second_reference_to_question_that_used_as_substitution_question = () =>
            verificationMessages.GetError("WB0019").References.ElementAt(1).Id.ShouldEqual(variableId);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static readonly Guid rosterId = Guid.Parse("13333333333333333333333333333333");
        private static readonly Guid questionId = Guid.Parse("10000000000000000000000000000000");
        private static readonly Guid variableId = Guid.Parse("12222222222222222222222222222222");
    }
}