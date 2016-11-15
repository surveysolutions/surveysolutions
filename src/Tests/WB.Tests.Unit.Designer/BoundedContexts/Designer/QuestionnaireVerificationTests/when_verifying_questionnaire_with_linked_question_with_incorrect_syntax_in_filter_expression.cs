using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Moq;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_linked_question_with_incorrect_syntax_in_filter_expression : QuestionnaireVerifierTestsContext
    {
        private Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocument(new IComposite[]
            {
                Create.FixedRoster(rosterId: rosterId, fixedTitles: new[] { "1", "2", "3" },children: new[]
                {
                    Create.NumericIntegerQuestion(variable: "enumeration_district"),
                }),

                Create.Group(groupId: groupId, children: new[]
                {
                    Create.SingleOptionQuestion(questionId: questionId, 
                    variable: "s546i",
                    linkedToRosterId: rosterId,
                    linkedFilterExpression: "incorrect [] expression")
                })
            });

            verifier = CreateQuestionnaireVerifier(expressionProcessorGenerator: CreateExpressionProcessorGenerator());
        };

        Because of = () =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        It should_return_1_message = () =>
            verificationMessages.Count().ShouldEqual(1);

        It should_return_message_with_code__WB0110 = () =>
            verificationMessages.First().Code.ShouldEqual("WB0110");

        It should_return_message_with_one_references = () =>
            verificationMessages.First().References.Count().ShouldEqual(1);

        It should_return_message_with_one_references_with_question_type = () =>
            verificationMessages.First().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_message_with_one_references_with_id_equals_questionId = () =>
            verificationMessages.First().References.First().Id.ShouldEqual(questionId);
        
        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid questionId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid groupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid rosterId = Guid.Parse("BAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

    }
}