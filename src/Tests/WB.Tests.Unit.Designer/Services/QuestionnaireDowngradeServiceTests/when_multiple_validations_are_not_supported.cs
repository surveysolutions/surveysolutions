using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.UI.Designer.Services;

namespace WB.Tests.Unit.Designer.Services.QuestionnaireDowngradeServiceTests
{
    [Subject(typeof(QuestionnaireDowngradeService))]
    internal class when_multiple_validations_are_not_supported
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            questionnaire = Create.QuestionnaireDocumentWithOneChapter(Create.TextQuestion(
                questionId: questionId,
                validationConditions: new List<ValidationCondition>
                {
                    new ValidationCondition("expression", "message")
                }));

            service = new QuestionnaireDowngradeService();
            BecauseOf();
        }

        private void BecauseOf() => service.Downgrade(questionnaire, 11);

        [NUnit.Framework.Test] public void should_put_validation_message_to_validation_message_field () => questionnaire.GetQuestion<TextQuestion>(questionId).ValidationMessage.ShouldEqual("message");

        static QuestionnaireDowngradeService service;
        static QuestionnaireDocument questionnaire;
        static Guid questionId;
    }
}