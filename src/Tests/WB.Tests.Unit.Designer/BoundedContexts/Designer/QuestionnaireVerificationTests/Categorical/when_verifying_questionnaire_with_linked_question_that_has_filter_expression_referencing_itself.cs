using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests.Categorical
{
    internal class when_verifying_questionnaire_with_linked_question_that_has_filter_expression_referencing_itself : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            var linkedSourceQuestionId = Guid.Parse("33333333333333333333333333333333");
            questionnaire = CreateQuestionnaireDocument(
                Create.FixedRoster(variable: "a",
                    fixedTitles: new[] {"fixed title 1", "fixed title 2"},
                    children: new IComposite[]
                    {
                        new TextQuestion()
                        {
                            PublicKey = linkedSourceQuestionId,
                            QuestionType = QuestionType.Text,
                            StataExportCaption = "var"
                        }
                    }),
                Create.SingleOptionQuestion(questionId: questionWithFilterId,
                    variable: "s546i",
                    linkedToQuestionId: linkedSourceQuestionId,
                    linkedFilterExpression: "s546i.Contains(1)")
                );

            var expressionProcessor = Mock.Of<IExpressionProcessor>(processor
                => processor.GetIdentifiersUsedInExpression(Moq.It.IsAny<string>()) == new[] { "s546i" });

            verifier = CreateQuestionnaireVerifier(expressionProcessor);
        };

        Because of = () =>
            resultErrors = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        It should_return_1_message = () =>
            resultErrors.Count().ShouldEqual(1);

        It should_return_message_with_code__WB0109 = () =>
            resultErrors.Single().Code.ShouldEqual("WB0109");

        It should_return_message_with_one_references = () =>
            resultErrors.Single().References.Count().ShouldEqual(1);

        It should_return_first_message_reference_with_type_Question = () =>
            resultErrors.Single().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_first_message_reference_with_id_of_question_with_enablement_condition = () =>
            resultErrors.Single().References.First().Id.ShouldEqual(questionWithFilterId);

        private static IEnumerable<QuestionnaireVerificationMessage> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid questionWithFilterId = Guid.Parse("10000000000000000000000000000000");
    }
}