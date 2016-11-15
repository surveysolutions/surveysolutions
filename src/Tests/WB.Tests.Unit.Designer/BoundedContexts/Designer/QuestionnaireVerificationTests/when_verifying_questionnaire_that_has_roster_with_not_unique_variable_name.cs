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
    internal class when_verifying_questionnaire_that_has_roster_with_not_unique_variable_name : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocument(new IComposite[]
            {
                Create.FixedRoster(rosterId: rosterId1,
                    fixedTitles: new[] {"1", "2"},
                    variable: nonUniqueVariableName,
                    children: new IComposite[]
                    {new TextListQuestion() {PublicKey = Guid.NewGuid(), StataExportCaption = "var1"}}),

                Create.FixedRoster(rosterId: rosterId2,
                    fixedTitles: new[] {"1", "2"},
                    variable: nonUniqueVariableName,
                    children: new IComposite[]
                    {new TextListQuestion() {PublicKey = Guid.NewGuid(), StataExportCaption = "var2"}})
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

        It should_return_message_with_first_references_with_Group_type = () =>
            verificationMessages.GetCritical("WB0026").References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Roster);

        It should_return_message_with_first_references_with_id_equals_rosterId1 = () =>
            verificationMessages.GetCritical("WB0026").References.First().Id.ShouldEqual(rosterId1);

        It should_return_message_with_second_references_with_Group_type = () =>
            verificationMessages.GetCritical("WB0026").References.Last().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Roster);

        It should_return_message_with_second_references_with_id_equals_rosterId2 = () =>
            verificationMessages.GetCritical("WB0026").References.Last().Id.ShouldEqual(rosterId2);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid rosterId1 = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid rosterId2 = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

        private static string nonUniqueVariableName = "variable";
    }
}
