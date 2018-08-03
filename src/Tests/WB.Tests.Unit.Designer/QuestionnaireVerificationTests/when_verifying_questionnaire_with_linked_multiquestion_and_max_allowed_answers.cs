using System;
using System.Collections.Generic;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.GenericSubdomains.Portable;
using QuestionnaireVerifier = WB.Core.BoundedContexts.Designer.Verifier.QuestionnaireVerifier;

namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests
{
    class when_verifying_questionnaire_that_have_linked_multiquestion_with_max_allowed_answers : QuestionnaireVerifierTestsContext
    {

        [NUnit.Framework.OneTimeSetUp] public void context () {
            multyOptionsQuestionId = Guid.Parse("10000000000000000000000000000000");
            var linkedQuestionId = Guid.Parse("20000000000000000000000000000000");
            var rosterSizeQuestion = Guid.Parse("30000000000000000000000000000000");
            var rosterGroup = Guid.Parse("40000000000000000000000000000000");
            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                    new NumericQuestion("Roster Size Question")
                    {
                        PublicKey = rosterSizeQuestion,
                        StataExportCaption = "var1",
                        IsInteger = true
                    },
                    new Group("Roster Group")
                    {
                        PublicKey = rosterGroup,
                        IsRoster = true,
                        VariableName = "a",
                        RosterSizeSource = RosterSizeSourceType.Question,
                        RosterSizeQuestionId = rosterSizeQuestion,
                        Children = new List<IComposite>()
                        {
                            Create.TextQuestion(
                                variable: "var2",
                                questionId: linkedQuestionId
                            )
                        }.ToReadOnlyCollection()
                    },
                    Create.MultyOptionsQuestion(
                        variable: "var3",
                        id: multyOptionsQuestionId,
                        maxAllowedAnswers: 3,
                        linkedToQuestionId: linkedQuestionId
                    ));

            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() => 
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_no_errors () =>
             verificationMessages.Should().BeEmpty();

        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;

        private static Guid multyOptionsQuestionId;
    }
}
