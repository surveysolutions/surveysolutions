using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireVerificationTests.Conditions
{
    internal class when_question_has_long_validation_condition : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            IList<ValidationCondition> validation = new List<ValidationCondition>();
            questionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            validation.Add(new ValidationCondition("self > 3", "rule1"));
            validation.Add(new ValidationCondition(new string('*', 201), "rule1"));
            questionnaire = CreateQuestionnaireDocumentWithOneChapter(Create.Question(
                questionId: questionId,
                validationConditions: validation));
        };

        Because of = () => errors = CreateQuestionnaireVerifier().Verify(questionnaire);

        It should_produce_WB0212_warning = () => errors.ShouldContainWarning("WB0212");

        It should_reference_to_question_with_long_condition = () => 
            errors.GetWarning("WB0212").References.First().Id.ShouldEqual(questionId);

        //It should_reference_to_long_condition = () => // KP-6826
        //    errors.GetWarning("WB0212").References.First().FailedValidationConditionIndex.ShouldEqual(1);

        static QuestionnaireDocument questionnaire;
        static IEnumerable<QuestionnaireVerificationMessage> errors;
        private static Guid questionId;
    }
}