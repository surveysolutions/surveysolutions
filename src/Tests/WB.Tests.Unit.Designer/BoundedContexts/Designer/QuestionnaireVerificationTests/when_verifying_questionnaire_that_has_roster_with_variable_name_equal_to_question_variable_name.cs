using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_that_has_roster_with_variable_name_equal_to_question_variable_name : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocument(new IComposite[]
            {
                Create.FixedRoster(rosterId: rosterId,
                    fixedTitles: new[] {"1", "2"},
                    variable: nonUniqueVariableName,
                    children: new IComposite[]
                    {new TextListQuestion() {PublicKey = Guid.NewGuid(), StataExportCaption = "var1"}}),
                new TextQuestion()
                {
                    PublicKey = questionId,
                    QuestionType = QuestionType.Text,
                    StataExportCaption = nonUniqueVariableName,
                    QuestionText = "text question"
                },
            });

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        It should_return_WB0026_error = () =>
           verificationMessages.ShouldContainCritical("WB0026");

        It should_return_message_with_level_critical = () =>
            verificationMessages.GetCritical("WB0026").MessageLevel.ShouldEqual(VerificationMessageLevel.Critical);

        It should_return_message_with_two_references = () =>
            verificationMessages.GetCritical("WB0026").References.Count().ShouldEqual(2);

        It should_return_message_with_first_references_with_Roster_type = () =>
            verificationMessages.GetCritical("WB0026").References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Roster);

        It should_return_message_with_first_references_with_id_equals_rosterId = () =>
            verificationMessages.GetCritical("WB0026").References.First().Id.ShouldEqual(rosterId);

        It should_return_message_with_second_references_with_Question_type = () =>
            verificationMessages.GetCritical("WB0026").References.Last().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_message_with_second_references_with_id_equals_questionId = () =>
            verificationMessages.GetCritical("WB0026").References.Last().Id.ShouldEqual(questionId);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid rosterId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");

        private static string nonUniqueVariableName = "variable";
    }
}
