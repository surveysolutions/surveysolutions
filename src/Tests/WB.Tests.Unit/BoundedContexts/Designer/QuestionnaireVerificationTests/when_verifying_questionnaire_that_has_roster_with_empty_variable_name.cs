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
    internal class when_verifying_questionnaire_that_has_roster_with_empty_variable_name : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocument(new IComposite[]
            {
                new Group
                {
                    PublicKey = rosterId,
                    IsRoster = true,
                    RosterFixedTitles = new[] { "1", "2" },
                    RosterSizeSource = RosterSizeSourceType.FixedTitles,
                    Children = new List<IComposite>() { new TextListQuestion() { PublicKey = Guid.NewGuid(), StataExportCaption = "var" } }
                }
            });

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            verificationMessages = verifier.Verify(questionnaire);

        It should_return_1_message = () =>
            verificationMessages.Count().ShouldEqual(1);

        It should_return_message_with_code_WB0067 = () =>
            verificationMessages.First().Code.ShouldEqual("WB0067");

        It should_return_message_with_one_references = () =>
            verificationMessages.First().References.Count().ShouldEqual(1);

        It should_return_message_with_one_references_with_Group_type = () =>
            verificationMessages.First().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Group);

        It should_return_message_with_one_references_with_id_equals_rosterId = () =>
            verificationMessages.First().References.First().Id.ShouldEqual(rosterId);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid rosterId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
    }
}
