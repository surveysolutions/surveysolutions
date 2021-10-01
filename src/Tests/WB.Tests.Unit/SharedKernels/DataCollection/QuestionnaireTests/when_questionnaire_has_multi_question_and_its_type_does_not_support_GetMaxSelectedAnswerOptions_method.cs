using System;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.DataCollection.QuestionnaireTests
{
    internal class when_questionnaire_has_multi_question_and_its_type_does_not_support_GetMaxSelectedAnswerOptions_method
        : QuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaireDocument = CreateQuestionnaireDocumentWithOneChapter(new IComposite[]
            {
                new MultyOptionsQuestion()
                {
                    PublicKey = validatedQuestionId,
                    MaxAllowedAnswers = proposedSelectedAnswerOptions
                }
            });
            
            BecauseOf();
        }

        public void BecauseOf() =>
            exception = Assert.Throws<QuestionnaireException>(() => Create.Entity.PlainQuestionnaire(questionnaireDocument, 1).GetMaxSelectedAnswerOptions(validatedQuestionId));

        [NUnit.Framework.Test] public void should_throw_questionnaire_exception () =>
            exception.Should().BeOfType<QuestionnaireException>();

        [NUnit.Framework.Test] public void should_throw_exception_with_message_containing__custom_validation__ () =>
            exception.Message.Should().Contain("Cannot return maximum for selected answers");

        private static int? proposedSelectedAnswerOptions = 5;
        private static Exception exception;
        private static QuestionnaireDocument questionnaireDocument;
        private static Guid validatedQuestionId = new Guid("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
    }
}
