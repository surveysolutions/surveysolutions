using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_question_and_group_with_incorrect_syntax_in_enablement_conditions_ : QuestionnaireVerifierTestsContext
    {
        private Establish context = () =>
        {
            const string incorrectConditionExpression = "[hehe] &=+< 5";
            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                new TextQuestion
                {
                    PublicKey = questionId,
                    ConditionExpression = incorrectConditionExpression,
                    StataExportCaption = "var1"
                },
                Create.Group(groupId: groupId, enablementCondition: incorrectConditionExpression)
            );
            
            verifier = CreateQuestionnaireVerifier(expressionProcessorGenerator: CreateExpressionProcessorGenerator());
        };

        Because of = () =>
            verificationMessages = verifier.CheckForErrors(questionnaire);

        It should_return_2_messages = () =>
            verificationMessages.Count().ShouldEqual(2);

        It should_return_each_error_with_code__WB0003__ = () =>
            verificationMessages.ShouldEachConformTo(error=> error.Code == "WB0003");

        It should_return_each_error_with_single_reference = () =>
            verificationMessages.ShouldEachConformTo(error=> error.References.Count() == 1);

        It should_return_one_error_referencing_with_type_of_question = () =>
            verificationMessages.Single(x => x.References.Single().Id == questionId)
                .References.Single().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_second_one_referencing_with_type_of_group = () =>
            verificationMessages.Single(x => x.References.Single().Id == groupId).
                References.Single().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Group);
        

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid groupId = Guid.Parse("22222222222222222222222222222222");
    }
}