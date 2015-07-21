using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_roster_with_illegal_variable_name : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            rosterId = Guid.Parse("10000000000000000000000000000000");
            questionnaire = CreateQuestionnaireDocument();


            var rosterSizeQiestionId = Guid.Parse("20000000000000000000000000000000");

            questionnaire.Children.Add(new NumericQuestion()
            {
                PublicKey = rosterSizeQiestionId,
                IsInteger = true,
                StataExportCaption = "var"
            });
            questionnaire.Children.Add(new Group()
            {
                PublicKey = rosterId,
                IsRoster = true,
                VariableName = "int",
                RosterSizeQuestionId = rosterSizeQiestionId,
                
            });


            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        It should_return_1_error = () =>
            resultErrors.Count().ShouldEqual(1);

        It should_return_error_with_code__WB0058 = () =>
            resultErrors.Single().Code.ShouldEqual("WB0058");

        It should_return_error_with_1_references = () =>
            resultErrors.Single().References.Count().ShouldEqual(1);

        It should_return_error_reference_with_type_Question = () =>
            resultErrors.Single().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Group);

        It should_return_error_reference_with_id_of_questionWithSelfSubstitutionsId = () =>
            resultErrors.Single().References.First().Id.ShouldEqual(rosterId);

        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid rosterId;
    }
}
