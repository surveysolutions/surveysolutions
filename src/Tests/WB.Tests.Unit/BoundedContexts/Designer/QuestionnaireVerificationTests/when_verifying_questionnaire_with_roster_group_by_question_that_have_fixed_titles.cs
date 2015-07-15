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
    internal class when_verifying_questionnaire_with_roster_group_by_question_that_have_fixed_titles : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            rosterGroupId = Guid.Parse("10000000000000000000000000000000");
            rosterSizeQuestionId = Guid.Parse("13333333333333333333333333333333");
            questionnaire = CreateQuestionnaireDocument();
            questionnaire.Children.Add(new NumericQuestion("question 1")
            {
                PublicKey = rosterSizeQuestionId,
                StataExportCaption = "var",
                IsInteger = true
            });
            questionnaire.Children.Add(new Group()
            {
                PublicKey = rosterGroupId,
                IsRoster = true,
                VariableName = "a",
                RosterSizeSource = RosterSizeSourceType.Question,
                RosterSizeQuestionId = rosterSizeQuestionId,
                RosterFixedTitles = new [] { "fixed title 1" }
            });
            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        It should_return_1_error = () =>
            resultErrors.Count().ShouldEqual(1);

        It should_return_error_with_code__WB0032__ = () =>
            resultErrors.Single().Code.ShouldEqual("WB0032");

        It should_return_error_with_1_references = () =>
            resultErrors.Single().References.Count().ShouldEqual(1);

        It should_return_error_reference_with_type_group = () =>
            resultErrors.Single().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Group);

        It should_return_error_reference_with_id_of_rosterGroupId = () =>
            resultErrors.Single().References.First().Id.ShouldEqual(rosterGroupId);

        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid rosterGroupId;
        private static Guid rosterSizeQuestionId;
    }
}
