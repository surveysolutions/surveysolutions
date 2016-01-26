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
    internal class when_verifying_questionnaire_in_which_particular_question_and_group_have_same_id : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionnaire = Create.QuestionnaireDocument(children: new IComposite[]
            {
                Create.Chapter(children: new IComposite[]
                {
                    Create.Group(groupId: sharedId),
                    Create.Group(groupId: Guid.Parse("22220000222255555555222200002222"), children: new IComposite[]
                    {
                        Create.Question(questionId: sharedId, variable: "var1"),
                    }),
                }),
            });

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        It should_return_1_error = () =>
            resultErrors.Count().ShouldEqual(1);

        It should_return_error_with_code_WB0102 = () =>
            resultErrors.Single().Code.ShouldEqual("WB0102");

        It should_return_error_with_level_critical = () =>
            resultErrors.Single().MessageLevel.ShouldEqual(VerificationMessageLevel.General);

        It should_return_error_with_2_references = () =>
            resultErrors.Single().References.Count.ShouldEqual(2);

        It should_return_error_with_reference_to_group = () =>
            resultErrors.Single().References.ShouldContain(reference => reference.Type == QuestionnaireVerificationReferenceType.Group);

        It should_return_error_with_reference_to_question = () =>
            resultErrors.Single().References.ShouldContain(reference => reference.Type == QuestionnaireVerificationReferenceType.Question);

        It should_return_error_with_references_having_same_shared_id = () =>
            resultErrors.Single().References.ShouldEachConformTo(reference => reference.Id == sharedId);

        private static QuestionnaireDocument questionnaire;
        private static QuestionnaireVerifier verifier;
        private static IEnumerable<QuestionnaireVerificationMessage> resultErrors;
        private static Guid sharedId = Guid.Parse("11111111111111111111111111111111");
    }
}