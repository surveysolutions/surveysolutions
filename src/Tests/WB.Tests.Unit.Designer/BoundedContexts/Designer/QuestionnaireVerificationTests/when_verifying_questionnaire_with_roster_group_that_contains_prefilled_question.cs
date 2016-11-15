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
using WB.Core.GenericSubdomains.Portable;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_roster_group_that_contains_prefilled_question : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            prefilledQuestionId = Guid.Parse("30000000000000000000000000000000");
            var rosterSizeQiestionId = Guid.Parse("20000000000000000000000000000000");

            questionnaire = CreateQuestionnaireDocument(
                new NumericQuestion()
                {
                    PublicKey = rosterSizeQiestionId,
                    IsInteger = true,
                    StataExportCaption = "var1"
                },
                new Group()
                {
                    PublicKey = Guid.Parse("10000000000000000000000000000000"),
                    IsRoster = true,
                    VariableName = "a",
                    RosterSizeQuestionId = rosterSizeQiestionId,
                    Children = new List<IComposite>()
                    {
                        new TextQuestion("Title"){ PublicKey = prefilledQuestionId, Featured = true, StataExportCaption = "var2" }
                    }.ToReadOnlyCollection()
                });

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        It should_return_1_message = () =>
            verificationMessages.Count().ShouldEqual(1);

        It should_return_message_with_code__WB0029__ = () =>
            verificationMessages.Single().Code.ShouldEqual("WB0030");

        It should_return_message_with_1_references = () =>
            verificationMessages.Single().References.Count().ShouldEqual(1);

        It should_return_message_reference_with_type_group = () =>
            verificationMessages.Single().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_message_reference_with_id_of_rosterGroupId = () =>
            verificationMessages.Single().References.First().Id.ShouldEqual(prefilledQuestionId);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid prefilledQuestionId;
    }
}
