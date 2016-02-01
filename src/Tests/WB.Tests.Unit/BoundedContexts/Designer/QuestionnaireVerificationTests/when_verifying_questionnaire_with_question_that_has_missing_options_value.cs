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
    internal class when_verifying_questionnaire_with_question_that_has_missing_options_value : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionWithMissingValues = Guid.Parse("10000000000000000000000000000000");

            questionnaire = CreateQuestionnaireDocument();
            questionnaire.Children.Add(new SingleQuestion()
            {
                PublicKey = questionWithMissingValues,
                StataExportCaption = "var",
                QuestionType = QuestionType.SingleOption,
                Answers = { new Answer() { AnswerText = "opt 1" }, new Answer() { AnswerValue = "2", AnswerText = "opt 2" } }
            });

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        It should_return_1_message = () =>
            resultErrors.Count().ShouldEqual(2);

        It should_return_message_with_code__WB0045 = () =>
            resultErrors.First().Code.ShouldEqual("WB0045");

        It should_return_message_with_one_reference = () =>
            resultErrors.First().References.Count().ShouldEqual(1);

        It should_return_first_error_reference_with_type_Question = () =>
            resultErrors.First().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_first_error_reference_with_id_of_questionWithCustomCondition = () =>
            resultErrors.First().References.First().Id.ShouldEqual(questionWithMissingValues);

        private static IEnumerable<QuestionnaireVerificationMessage> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid questionWithMissingValues;
    }
}
