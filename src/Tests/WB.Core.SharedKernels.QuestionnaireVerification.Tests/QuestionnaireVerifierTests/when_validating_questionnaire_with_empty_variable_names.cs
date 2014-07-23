using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.QuestionnaireVerification.Implementation.Services;
using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects;

namespace WB.Core.SharedKernels.QuestionnaireVerification.Tests.QuestionnaireVerifierTests
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

        private Because of = () => errors = verifier.Verify(questionnaire).ToList();

        private It should_skip_empty_names_when_validating_uniqueness = () => errors.Where(x => x.Code == "WB0062").ShouldBeEmpty();

        private static QuestionnaireDocument questionnaire;
        private static QuestionnaireVerifier verifier;
        private static IEnumerable<QuestionnaireVerificationError> errors;
    }
}

