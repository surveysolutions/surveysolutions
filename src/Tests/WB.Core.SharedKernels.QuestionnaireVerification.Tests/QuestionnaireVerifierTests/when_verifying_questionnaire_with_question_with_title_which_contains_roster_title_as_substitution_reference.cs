﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.QuestionnaireVerification.Implementation.Services;
using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects;

namespace WB.Core.SharedKernels.QuestionnaireVerification.Tests.QuestionnaireVerifierTests
{
    internal class when_verifying_questionnaire_with_question_with_title_which_contains_roster_title_as_substitution_reference : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionId = Guid.Parse("10000000000000000000000000000000");
            questionnaire = CreateQuestionnaireDocument();

            questionnaire.Children.Add(new SingleQuestion()
            {
                PublicKey = questionId,
                StataExportCaption = "var",
                QuestionText = "hello %rostertitle%",
                Answers = { new Answer() { AnswerValue = "1", AnswerText = "opt 1" }, new Answer() { AnswerValue = "2", AnswerText = "opt 2" } }
            });

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        It should_return_1_error = () =>
            resultErrors.Count().ShouldEqual(1);

        It should_return_error_with_code__WB0059 = () =>
            resultErrors.Single().Code.ShouldEqual("WB0059");

        It should_return_error_with_1_references = () =>
            resultErrors.Single().References.Count().ShouldEqual(1);

        It should_return_error_reference_with_type_Question = () =>
            resultErrors.Single().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_error_reference_with_id_of_questionWithSelfSubstitutionsId = () =>
            resultErrors.Single().References.First().Id.ShouldEqual(questionId);

        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid questionId;
    }
}
