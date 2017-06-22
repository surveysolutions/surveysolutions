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
    internal class when_verifying_questionnaire_with_group_where_roster_size_source_is_question_that_does_not_have_roster_size_question :
        QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {

            rosterGroupId = Guid.Parse("13333333333333333333333333333333");
            questionnaire = CreateQuestionnaireDocumentWithOneChapter(new Group()
                {
                    PublicKey = rosterGroupId,
                    IsRoster = true,
                    RosterSizeSource = RosterSizeSourceType.Question,
                    VariableName = "a",
                    Children = new List<IComposite>()
                    {
                        Create.NumericIntegerQuestion(variable: "var")
                    }.ToReadOnlyCollection()
            });

            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_1_message () =>
            verificationMessages.Count().ShouldEqual(1);

        [NUnit.Framework.Test] public void should_return_first_error_with_code__WB0009 () =>
            verificationMessages.ShouldContainError("WB0009");

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid rosterGroupId;
    }
}
