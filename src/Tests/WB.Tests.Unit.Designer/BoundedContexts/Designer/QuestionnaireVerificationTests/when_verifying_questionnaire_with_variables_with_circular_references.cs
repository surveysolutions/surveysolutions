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
using WB.Core.SharedKernels.QuestionnaireEntities;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_variables_with_circular_references : QuestionnaireVerifierTestsContext
    {
        private Establish context = () =>
        {
            questionnaire = Create.QuestionnaireDocument(children: new IComposite[]
            {
                Create.Chapter(children: new IComposite[]
                {
                    Create.Variable(variable1Id, VariableType.LongInteger, "v1", expression: "v2 > 10"),
                    Create.Variable(variable2Id, VariableType.LongInteger, "v2", expression: "v1 == 8")
                })
            });

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        It should_return_contain_error_WB0056 = () =>
            verificationMessages.ShouldContainError("WB0056");

        It should_return_message_with_level_general = () =>
            verificationMessages.GetError("WB0056").MessageLevel.ShouldEqual(VerificationMessageLevel.General);

        It should_return_message_with_two_references = () =>
            verificationMessages.GetError("WB0056").References.Count().ShouldEqual(2);

        It should_return_message_with_references_on_veriable1_and_2 = () =>
            verificationMessages.GetError("WB0056").References.Select(x => x.Id).ShouldContainOnly(variable1Id, variable2Id);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static readonly Guid variable1Id = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static readonly Guid Question1Id = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static readonly Guid variable2Id = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
    }
}