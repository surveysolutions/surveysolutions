using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests.Categorical
{
    internal class when_categorical_question_has_more_than_30_options : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            List<Answer> answers = new List<Answer>(31);
            for (int i = 0; i <= 30; i++)
            {
                answers.Add(Create.Answer("answer" + i, i));
            }

            questionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            questionnaire = CreateQuestionnaireDocumentWithOneChapter(Create.Question(questionId:
                questionId,
                questionType: QuestionType.SingleOption,
                answers: answers.ToArray()));

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () => errors = verifier.Verify(Create.QuestionnaireView(questionnaire));

        It should_contain_WB0210_warning = () => errors.ShouldContainWarning("WB0210");

        It should_referece_to_question_with_warning = () =>
            errors.GetWarning("WB0210").References.ShouldContainOnly(Create.VerificationReference(id: questionId));

        static QuestionnaireDocument questionnaire;
        static QuestionnaireVerifier verifier;
        static IEnumerable<QuestionnaireVerificationMessage> errors;
        static Guid questionId;
    }
}