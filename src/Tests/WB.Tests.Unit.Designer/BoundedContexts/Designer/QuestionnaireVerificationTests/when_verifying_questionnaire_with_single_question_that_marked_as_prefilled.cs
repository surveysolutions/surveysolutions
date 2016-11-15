using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_single_question_that_marked_as_prefilled : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocument(new[]
            {
                Create.Group(groupId, children: new IComposite[]
                {
                    Create.SingleQuestion(questionId, scope: QuestionScope.Headquarter, isPrefilled: true,  optionsFilter: "(@optioncode == 100) || (@optioncode == 200)")
                })
            });

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        It should_return_WB0029_message = () =>
            verificationMessages.ShouldContainError("WB0029");

        It should_return_message_with_one_references = () =>
            verificationMessages.GetError("WB0029").References.Count().ShouldEqual(1);

        It should_return_message_with_one_references_with_question_type = () =>
            verificationMessages.GetError("WB0029").References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_message_with_one_references_with_id_equals_questionId = () =>
            verificationMessages.GetError("WB0029").References.First().Id.ShouldEqual(questionId);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static readonly Guid questionId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static readonly Guid groupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
    }
}