﻿using System;
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
    class when_verifying_questionnaire_with_multianswer_invalid_max_allowed_answers : QuestionnaireVerifierTestsContext
    {

        Establish context = () =>
        {
            multyOptionsQuestionId = Guid.Parse("10000000000000000000000000000000");
            questionnaire = CreateQuestionnaireDocument();
            
            questionnaire.Children.Add(new MultyOptionsQuestion()
            {
                PublicKey = multyOptionsQuestionId,
                StataExportCaption = "var",
                Answers = new List<Answer>() { new Answer() { AnswerValue = "1", AnswerText = "Hello, 1"},
                    new Answer() { AnswerValue = "2", AnswerText = "Hello, 2" } },
                MaxAllowedAnswers = 3
            });

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () => 
            resultErrors = verifier.Verify(questionnaire);

        It should_return_1_errors = () => 
            resultErrors.Count().ShouldEqual(1);

        It should_return_error_with_code__WB0021__ = () =>
            resultErrors.Single().Code.ShouldEqual("WB0021");

        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static IEnumerable<QuestionnaireVerificationError> resultErrors;

        private static Guid multyOptionsQuestionId;
    }
}
