using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_roster_has_single_question : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            rosterId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            questionnaire = CreateQuestionnaireDocumentWithOneChapter(Create.Roster(
                rosterId: rosterId,
                children: new[] {Create.Question()}));
            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () => { errors = verifier.Verify(Create.QuestionnaireView(questionnaire)); };

        It should_contain_WB0203_warning = () => errors.ShouldContain(item => item.Code == "WB0203" && item.MessageLevel == VerificationMessageLevel.Warning);

        It should_reference_to_a_roster = () => errors.FirstOrDefault(item => item.Code == "WB0203")
            .References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Roster);

        It should_reference_to_a_roster_with_single_question = () => errors.FirstOrDefault(item => item.Code == "WB0203")
            .References.First().Id.ShouldEqual(rosterId);

        It should_add_single_reference = () => errors.FirstOrDefault(item => item.Code == "WB0203").References.Count.ShouldEqual(1);

        static QuestionnaireVerifier verifier;
        static QuestionnaireDocument questionnaire;
        static IEnumerable<QuestionnaireVerificationMessage> errors;
        static Guid rosterId;
    }
}