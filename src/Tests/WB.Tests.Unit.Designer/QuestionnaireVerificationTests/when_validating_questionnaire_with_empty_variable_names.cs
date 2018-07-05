using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using QuestionnaireVerifier = WB.Core.BoundedContexts.Designer.Verifier.QuestionnaireVerifier;

namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests
{
    internal class when_validating_questionnaire_with_empty_variable_names : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {

            questionnaire = CreateQuestionnaireDocumentWithOneChapter(new NumericQuestion
            {
                PublicKey = Guid.NewGuid(),
                StataExportCaption = null
            },
                new NumericQuestion
                {
                    PublicKey = Guid.NewGuid(),
                    StataExportCaption = null
                },
                new NumericQuestion
                {
                    PublicKey = Guid.NewGuid(),
                    StataExportCaption = ""
                },
                new NumericQuestion
                {
                    PublicKey = Guid.NewGuid(),
                    StataExportCaption = ""
                }
                );
            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() => 
            verificationMessages = Enumerable.ToList<QuestionnaireVerificationMessage>(verifier.CheckForErrors(Create.QuestionnaireView(questionnaire)));

        [NUnit.Framework.Test] public void should_skip_empty_names_when_validating_uniqueness () => 
            verificationMessages.Where(x => x.Code == "WB0062").Should().BeEmpty();

        private static QuestionnaireDocument questionnaire;
        private static QuestionnaireVerifier verifier;
        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
    }
}

