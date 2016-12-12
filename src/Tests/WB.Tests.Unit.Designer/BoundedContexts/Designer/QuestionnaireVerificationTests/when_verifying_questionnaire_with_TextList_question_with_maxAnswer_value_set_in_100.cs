using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_TextList_question_with_maxAnswer_value_set_in_100 : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionnaire = Create.QuestionnaireDocument(children: new IComposite[]
            {
                Create.Chapter(children: new IComposite[]
                {
                    Create.TextListQuestion(textQuestion1Id, maxAnswerCount: 250),
                    Create.TextListQuestion(textQuestion2Id, maxAnswerCount: null)
                })
            });
           
            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        It should_return_error__WB0042 = () =>
            verificationMessages.ShouldContainError("WB0042");

        It should_return_message_with_level_general = () =>
            verificationMessages.GetError("WB0042").MessageLevel.ShouldEqual(VerificationMessageLevel.General);

        It should_return_message_with_1_references = () =>
            verificationMessages.GetError("WB0042").References.Count().ShouldEqual(1);

        It should_return_message_reference_with_type_Question = () =>
            verificationMessages.GetError("WB0042").References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_message_reference_with_id_of_textQuestion1Id = () =>
            verificationMessages.GetError("WB0042").References.First().Id.ShouldEqual(textQuestion1Id);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static readonly Guid textQuestion1Id = Guid.Parse("10000000000000000000000000000000");
        private static readonly Guid textQuestion2Id = Guid.Parse("20000000000000000000000000000000");
    }
}