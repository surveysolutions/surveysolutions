using System;
using System.Linq;
using Main.Core.Entities.Composite;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Questionnaire.Categories;

namespace WB.Tests.Unit.Designer.Services
{
    [TestOf(typeof(CategoricalOptionsImportService))]
    internal class CategoricalOptionsImportServiceTests
    {
        [Test]
        public void when_ImportOptions_for_categorical_cascading_question_and_parent_question_with_empty_reusable_categories_then_should_return_failed_ImportCategoricalOptionsResult()
        {
            // arrange
            var questionnaireId = Guid.Parse("11111111111111111111111111111111");
            var questionId = Guid.Parse("22222222222222222222222222222222");
            var comboboxQuestionId = Guid.Parse("12345678901234567890123456789012");
            var categoriesId = Guid.Parse("33333333333333333333333333333333");
            var questionnaire = Create.QuestionnaireDocumentWithOneChapter(questionnaireId: questionnaireId,
                children: new IComposite[]
                {
                    Create.SingleOptionQuestion(
                        questionId: comboboxQuestionId,
                        isComboBox: true,
                        variable: "parentCombobox",
                        categoriesId: categoriesId),
                    Create.SingleOptionQuestion(questionId: questionId, cascadeFromQuestionId: comboboxQuestionId),
                });

            var categoriesService = Mock.Of<ICategoriesService>(x =>
                x.GetCategoriesById(It.IsAny<Guid>(), categoriesId) == Array.Empty<CategoriesItem>().AsQueryable());

            var service = Create.CategoricalOptionsImportService(questionnaire, categoriesService);

            // act
            var result = service.ImportOptions("1\tStreet 1\t2".GenerateStream(), questionnaireId.FormatGuid(), questionId);

            Assert.That(result.Errors, Has.One.Items);
            Assert.That(result.Errors.First(), Is.EqualTo("No categories for parent cascading question 'parentCombobox' found"));
        }

        [Test]
        public void when_ImportOptions_for_categorical_cascading_question_and_parent_question_with_reusable_categories_then_should_return_success_ImportCategoricalOptionsResult()
        {
            // arrange
            var questionnaireId = Guid.Parse("11111111111111111111111111111111");
            var questionId = Guid.Parse("22222222222222222222222222222222");
            var comboboxQuestionId = Guid.Parse("12345678901234567890123456789012");
            var categoriesId = Guid.Parse("33333333333333333333333333333333");
            var questionnaire = Create.QuestionnaireDocumentWithOneChapter(questionnaireId: questionnaireId,
                children: new IComposite[]
                {
                    Create.SingleOptionQuestion(
                        questionId: comboboxQuestionId,
                        isComboBox: true,
                        variable: "parentCombobox",
                        categoriesId: categoriesId),
                    Create.SingleOptionQuestion(questionId: questionId, cascadeFromQuestionId: comboboxQuestionId),
                });

            var categoriesService = Mock.Of<ICategoriesService>(x =>
                x.GetCategoriesById(It.IsAny<Guid>(), categoriesId) == new[] {new CategoriesItem {Id = 1, Text = "parent 1"}}.AsQueryable());

            var service = Create.CategoricalOptionsImportService(questionnaire, categoriesService);

            // act
            var result = service.ImportOptions("1\tStreet 1\t1".GenerateStream(), questionnaireId.FormatGuid(), questionId);

            Assert.That(result.Succeeded, Is.True);
        }

        [Test]
        public void when_ImportOptions_for_categorical_cascading_question_and_parent_question_with_regular_categories_then_should_return_success_ImportCategoricalOptionsResult()
        {
            // arrange
            var questionnaireId = Guid.Parse("11111111111111111111111111111111");
            var questionId = Guid.Parse("22222222222222222222222222222222");
            var comboboxQuestionId = Guid.Parse("12345678901234567890123456789012");
            var questionnaire = Create.QuestionnaireDocumentWithOneChapter(questionnaireId: questionnaireId,
                children: new IComposite[]
                {
                    Create.SingleOptionQuestion(
                        questionId: comboboxQuestionId,
                        isComboBox: true,
                        variable: "parentCombobox",
                        answerCodes: new[] {1m, 2m, 3m}),
                    Create.SingleOptionQuestion(questionId: questionId, cascadeFromQuestionId: comboboxQuestionId),
                });

            var service = Create.CategoricalOptionsImportService(questionnaire);

            // act
            var result = service.ImportOptions("1\tStreet 1\t1".GenerateStream(), questionnaireId.FormatGuid(), questionId);

            Assert.That(result.Succeeded, Is.True);
        }
    }
}
