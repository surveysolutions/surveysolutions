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

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_roster_group_that_contains_prefilled_question : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            prefilledQuestionId = Guid.Parse("30000000000000000000000000000000");
            questionnaire = CreateQuestionnaireDocument();

            var rosterSizeQiestionId = Guid.Parse("20000000000000000000000000000000");

            questionnaire.Children.Add(new NumericQuestion()
            {
                PublicKey = rosterSizeQiestionId,
                IsInteger = true,
                StataExportCaption = "var1"
            });
            questionnaire.Children.Add(new Group()
            {
                PublicKey = Guid.Parse("10000000000000000000000000000000"),
                IsRoster = true,
                VariableName = "a",
                RosterSizeQuestionId = rosterSizeQiestionId,
                Children = new List<IComposite>()
                {
                    new TextQuestion("Title"){ PublicKey = prefilledQuestionId, Featured = true, StataExportCaption = "var2" }
                }
            });

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        It should_return_1_error = () =>
            resultErrors.Count().ShouldEqual(1);

        It should_return_error_with_code__WB0029__ = () =>
            resultErrors.Single().Code.ShouldEqual("WB0030");

        It should_return_error_with_1_references = () =>
            resultErrors.Single().References.Count().ShouldEqual(1);

        It should_return_error_reference_with_type_group = () =>
            resultErrors.Single().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_error_reference_with_id_of_rosterGroupId = () =>
            resultErrors.Single().References.First().Id.ShouldEqual(prefilledQuestionId);

        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid prefilledQuestionId;
    }
}
