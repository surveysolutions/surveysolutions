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
    class when_verifying_questionnaire_with_categorical_multi_answers_question_that_has_max_allowed_answers_less_than_2 : QuestionnaireVerifierTestsContext
    {

        Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocument();
            
            questionnaire.Children.Add(new MultyOptionsQuestion()
            {
                PublicKey = multyOptionsQuestionId,
                StataExportCaption = "var",
                Answers = new List<Answer>() { new Answer() { AnswerValue = "2", AnswerText = "2" }, new Answer() { AnswerValue = "1", AnswerText = "1" } },
                MaxAllowedAnswers = -1
            });

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () => 
            verificationMessages = verifier.CheckForErrors(questionnaire);

        It should_return_1_message = () => 
            verificationMessages.Count().ShouldEqual(1);

        It should_return_message_with_code__WB0061__ = () =>
            verificationMessages.Single().Code.ShouldEqual("WB0061");

        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;

        private static Guid multyOptionsQuestionId = Guid.Parse("10000000000000000000000000000000");
    }
}
