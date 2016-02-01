using System;
using System.Collections.Generic;
using System.Linq;

using Machine.Specifications;

using Main.Core.Documents;
using Main.Core.Entities.Composite;

using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_questionnaire_contains_21_rosters : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionnaire = Create.QuestionnaireDocument(Guid.NewGuid(), children: new IComposite[]
            {
                Create.Roster(variable:"r1"),
                Create.Roster(variable:"r2"),
                Create.Roster(variable:"r3"),
                Create.Roster(variable:"r4"),
                Create.Roster(variable:"r5"),
                Create.Roster(variable:"r6"),
                Create.Roster(variable:"r7"),
                Create.Roster(variable:"r8"),
                Create.Roster(variable:"r9"),
                Create.Roster(variable:"r10"),
                Create.Roster(variable:"r11"),
                Create.Roster(variable:"r12"),
                Create.Roster(variable:"r13"),
                Create.Roster(variable:"r14"),
                Create.Roster(variable:"r15"),
                Create.Roster(variable:"r16"),
                Create.Roster(variable:"r17"),
                Create.Roster(variable:"r18"),
                Create.Roster(variable:"r19"),
                Create.Roster(variable:"r20"),
                Create.Roster(variable:"r21"),
                Create.NumericIntegerQuestion(Guid.NewGuid(), variable: "numeric")
            });
            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            verificationMessages = verifier.Verify(questionnaire);

        It should_return_1_message = () =>
            verificationMessages.Count().ShouldEqual(1);

        It should_return_message_with_code_WB0200 = () =>
            verificationMessages.First().Code.ShouldEqual("WB0200");

        It should_return_message_with_Warning_level = () =>
            verificationMessages.First().MessageLevel.ShouldEqual(VerificationMessageLevel.Warning);

        It should_return_message_with_no_references = () =>
            verificationMessages.First().References.Count().ShouldEqual(0);

        static QuestionnaireDocument questionnaire;
        static QuestionnaireVerifier verifier;
        static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
    }
}