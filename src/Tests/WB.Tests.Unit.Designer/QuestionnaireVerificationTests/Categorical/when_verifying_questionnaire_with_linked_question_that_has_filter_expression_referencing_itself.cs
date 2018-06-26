using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using QuestionnaireVerifier = WB.Core.BoundedContexts.Designer.Verifier.QuestionnaireVerifier;


namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests.Categorical
{
    [TestFixture]
    internal class when_verifying_questionnaire_with_linked_question_that_has_filter_expression_referencing_itself : QuestionnaireVerifierTestsContext
    {
        [Test]
        public void context()
        {
            var linkedSourceQuestionId = Guid.Parse("33333333333333333333333333333333");
            questionnaire = CreateQuestionnaireDocument(
                Create.FixedRoster(variable: "a",
                    fixedTitles: new[] { "fixed title 1", "fixed title 2" },
                    children: new IComposite[]
                    {
                        Create.TextQuestion(
                            linkedSourceQuestionId,
                            variable: "var"
                        )
                    }),
                Create.SingleOptionQuestion(questionId: questionWithFilterId,
                    variable: "s546i",
                    linkedToQuestionId: linkedSourceQuestionId,
                    linkedFilterExpression: "s546i.Contains(1)")
                );

            var expressionProcessor = Mock.Of<IExpressionProcessor>(processor
                => processor.GetIdentifiersUsedInExpression(Moq.It.IsAny<string>()) == new[] { "s546i" });

            verifier = CreateQuestionnaireVerifier(expressionProcessor);
            resultErrors = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

            Assert.That(resultErrors, Is.Not.Null);

            Assert.That(resultErrors.GetError("WB0109"), Is.Not.Null);
            // should_return_message_with_one_references() =>
            Assert.That(resultErrors.GetError("WB0109").References.Count(), Is.EqualTo(1));
            // should_return_first_message_reference_with_type_Question() =>
            Assert.That(resultErrors.GetError("WB0109").References.Single().Type, Is.EqualTo(QuestionnaireVerificationReferenceType.Question));
            // should_return_first_message_reference_with_id_of_question_with_enablement_condition() =>
            Assert.That(resultErrors.GetError("WB0109").References.Single().Id, Is.EqualTo(questionWithFilterId));

            Assert.That(resultErrors.GetError("WB0056"), Is.Not.Null);
            Assert.That(resultErrors.GetError("WB0056").References.Count(), Is.EqualTo(1));
            Assert.That(resultErrors.GetError("WB0056").References.Single().Type, Is.EqualTo(QuestionnaireVerificationReferenceType.Question));
            Assert.That(resultErrors.GetError("WB0056").References.Single().Id, Is.EqualTo(questionWithFilterId));
        }


        private static IEnumerable<QuestionnaireVerificationMessage> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid questionWithFilterId = Guid.Parse("10000000000000000000000000000000");
    }
}