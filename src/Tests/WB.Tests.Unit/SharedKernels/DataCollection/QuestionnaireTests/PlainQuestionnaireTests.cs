using System;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.QuestionnaireTests
{
    [TestOf(typeof(PlainQuestionnaire))]
    internal partial class PlainQuestionnaireTests : QuestionnaireTestsContext
    {
        [Test]
        public void when_GetMaxSelectedAnswerOptions_and_multi_combobox_question_without_max_answers_count_then_max_answers_count_should_be_200()
        {
            // arrange
            Guid validatedQuestionId = new Guid("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            var questionnaireDocument = CreateQuestionnaireDocumentWithOneChapter(new MultyOptionsQuestion
            {
                PublicKey = validatedQuestionId
            });

            // act
            var maxSelectedAnswerOptions = Create.Entity.PlainQuestionnaire(questionnaireDocument, 1)
                .GetMaxSelectedAnswerOptions(validatedQuestionId);

            // assert
            Assert.That(maxSelectedAnswerOptions, Is.Null);
        }
    }
}
