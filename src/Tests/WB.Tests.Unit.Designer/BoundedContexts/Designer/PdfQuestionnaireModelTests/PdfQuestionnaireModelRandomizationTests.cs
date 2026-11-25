using System;
using System.Collections.Generic;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.PdfQuestionnaireModelTests
{
    [TestOf(typeof(PdfQuestionnaireModel))]
    internal class PdfQuestionnaireModelRandomizationTests
    {
        [Test]
        public void GetFormattedQuestionType_for_single_option_question_with_randomized_options_should_mention_randomized_options()
        {
            var question = Create.SingleOptionQuestion(Guid.NewGuid());
            question.RandomizeAnswerOptions = true;
            var model = CreateModel(Create.QuestionnaireDocumentWithOneChapter(Guid.NewGuid(), question));

            Assert.That(model.GetFormattedQuestionType(question), Does.Contain(PdfStrings.QuestionType_RandomizedOptions));
        }

        [Test]
        public void GetFormattedQuestionType_for_multi_option_question_with_randomized_options_should_mention_randomized_options()
        {
            var question = Create.MultyOptionsQuestion(Guid.NewGuid());
            question.RandomizeAnswerOptions = true;
            var model = CreateModel(Create.QuestionnaireDocumentWithOneChapter(Guid.NewGuid(), question));

            Assert.That(model.GetFormattedQuestionType(question), Does.Contain(PdfStrings.QuestionType_RandomizedOptions));
        }

        [Test]
        public void GetFormattedQuestionType_for_question_without_randomized_options_should_not_mention_randomized_options()
        {
            var question = Create.SingleOptionQuestion(Guid.NewGuid());
            var model = CreateModel(Create.QuestionnaireDocumentWithOneChapter(Guid.NewGuid(), question));

            Assert.That(model.GetFormattedQuestionType(question), Does.Not.Contain(PdfStrings.QuestionType_RandomizedOptions));
        }

        [Test]
        public void AreAnswerOptionsRandomized_should_be_true_only_for_categorical_questions_with_randomized_options()
        {
            var randomizedSingle = Create.SingleOptionQuestion(Guid.NewGuid());
            randomizedSingle.RandomizeAnswerOptions = true;
            var randomizedMulti = Create.MultyOptionsQuestion(Guid.NewGuid());
            randomizedMulti.RandomizeAnswerOptions = true;
            var plainSingle = Create.SingleOptionQuestion(Guid.NewGuid());
            var randomizedCombobox = Create.SingleOptionQuestion(Guid.NewGuid());
            randomizedCombobox.RandomizeAnswerOptions = true;
            randomizedCombobox.IsFilteredCombobox = true;

            var model = CreateModel(Create.QuestionnaireDocumentWithOneChapter(Guid.NewGuid(),
                randomizedSingle, randomizedMulti, plainSingle, randomizedCombobox));

            Assert.That(model.AreAnswerOptionsRandomized(randomizedSingle), Is.True);
            Assert.That(model.AreAnswerOptionsRandomized(randomizedMulti), Is.True);
            Assert.That(model.AreAnswerOptionsRandomized(plainSingle), Is.False);
            Assert.That(model.AreAnswerOptionsRandomized(randomizedCombobox), Is.False);
        }

        [Test]
        public void IsOrderRandomized_should_be_true_only_for_entities_marked_with_randomize_order()
        {
            var randomizedQuestion = Create.TextQuestion(Guid.NewGuid());
            randomizedQuestion.RandomizeOrder = true;
            var plainQuestion = Create.TextQuestion(Guid.NewGuid());
            var randomizedStaticText = Create.StaticText(Guid.NewGuid());
            randomizedStaticText.RandomizeOrder = true;
            var randomizedGroup = Create.Group(Guid.NewGuid());
            randomizedGroup.RandomizeOrder = true;

            var model = CreateModel(Create.QuestionnaireDocumentWithOneChapter(Guid.NewGuid(),
                randomizedQuestion, plainQuestion, randomizedStaticText, randomizedGroup));

            Assert.That(model.IsOrderRandomized(randomizedQuestion), Is.True);
            Assert.That(model.IsOrderRandomized(plainQuestion), Is.False);
            Assert.That(model.IsOrderRandomized(randomizedStaticText), Is.True);
            Assert.That(model.IsOrderRandomized(randomizedGroup), Is.True);
        }

        [Test]
        public void IsOrderRandomized_should_be_false_for_entities_on_cover_page()
        {
            var coverQuestion = Create.TextQuestion(Guid.NewGuid());
            coverQuestion.RandomizeOrder = true;

            var model = CreateModel(Create.QuestionnaireDocumentWithCoverPage(Guid.NewGuid(), coverQuestion));

            Assert.That(model.IsOrderRandomized(coverQuestion), Is.False);
        }

        private static PdfQuestionnaireModel CreateModel(QuestionnaireDocument questionnaire)
        {
            questionnaire.ConnectChildrenWithParent();
            var user = new PdfQuestionnaireModel.ModificationStatisticsByUser { UserId = Guid.NewGuid(), Name = "user", Date = DateTime.UtcNow };
            return new PdfQuestionnaireModel(questionnaire, new PdfSettings(), new List<IComposite>(), user, user, user);
        }
    }
}
