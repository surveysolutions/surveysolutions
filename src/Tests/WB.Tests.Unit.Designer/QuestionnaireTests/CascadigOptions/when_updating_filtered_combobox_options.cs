using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Tests.Abc;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.QuestionnaireTests.CascadigOptions
{
    internal class when_updating_filtered_combobox_options : QuestionnaireTestsContext
    {
        [Test]
        public void should_allow_negative_values()
        {
            var questionId = Id.g1;
            var questionnaireDocument =
                Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Abc.Create.Entity.SingleOptionQuestion(
                        questionId: questionId,
                        isFilteredCombobox: true
                        ));

            var questionnaire = Create.Questionnaire(Id.gA, questionnaireDocument);

            // Act
            questionnaire.UpdateFilteredComboboxOptions(questionId, Id.gA, new Option[]
            {
                new Option("-1", "m 1"), 
            });

            var categoricalQuestion = questionnaireDocument.Find<ICategoricalQuestion>(questionId);
            Assert.That(categoricalQuestion.Answers, Has.Count.EqualTo(1));
            Assert.That(categoricalQuestion.Answers[0], Has.Property(nameof(Answer.AnswerValue)).EqualTo("-1"));
        }
    }
}
