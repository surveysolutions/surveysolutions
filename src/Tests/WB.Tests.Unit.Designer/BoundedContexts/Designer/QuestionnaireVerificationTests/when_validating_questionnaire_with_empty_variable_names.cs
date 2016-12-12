using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    [Subject(typeof(QuestionnaireVerifier), "WB0062")]
    internal class when_validating_questionnaire_with_empty_variable_names : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {

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
        };

        Because of = () => 
            verificationMessages = Enumerable.ToList<QuestionnaireVerificationMessage>(verifier.CheckForErrors(Create.QuestionnaireView(questionnaire)));

        It should_skip_empty_names_when_validating_uniqueness = () => 
            verificationMessages.Where(x => x.Code == "WB0062").ShouldBeEmpty();

        private static QuestionnaireDocument questionnaire;
        private static QuestionnaireVerifier verifier;
        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
    }
}

