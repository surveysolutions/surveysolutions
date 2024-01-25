using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using NHibernate.Criterion;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.Questionnaire.Categories;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.SharedKernels.Enumerator.OptionsRepositoryTests
{
    [TestFixture(TestOf = typeof(OptionsRepository))]
    public class OptionsRepositoryTests
    {
        [Test]
        public void should_keep_options_order_after_saving_options()
        {
            var questionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var answerCodes = Enumerable.Range(0, 300).Reverse().ToArray();

            SingleQuestion question = Create.Entity.SingleOptionQuestion(
                questionId: questionId,
                variable: "cat",
                answerCodes: answerCodes,
                isFilteredCombobox: true);

            var storage = Create.Storage.OptionsRepository(new SqliteInmemoryStorage<OptionView, int?>());

            var questionnaireIdentity = Create.Entity.QuestionnaireIdentity();
            storage.StoreOptionsForQuestion(questionnaireIdentity, questionId, question.Answers, new List<TranslationDto>());

            var filteredQuestionOptions = storage.GetFilteredQuestionOptions(questionnaireIdentity, questionId, null, null, null);

            var actual = filteredQuestionOptions.Select(x => x.Value).ToList();
            Assert.That(actual, Is.Not.Empty);
            Assert.That(actual, Is.Ordered.Descending);
        }

        [Test]
        public void should_return_options_respecting_translation()
        {
            var questionnaireIdentity = Create.Entity.QuestionnaireIdentity();

            var questionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            var options = new List<Answer>();
            options.Add(Create.Entity.Answer(1.ToString(), 1));
            options.Add(Create.Entity.Answer(2.ToString(), 2));
            options.Add(Create.Entity.Answer(3.ToString(), 3));


            SingleQuestion question = Create.Entity.SingleQuestion(
                id: questionId,
                variable: "cat",
                options: options,
                isFilteredCombobox: true);

            var translationId = Guid.Parse("1AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            var translations = new List<TranslationDto>()
            {
                // KP-13585 both scenarios should be supported with $ and without $ at the end
                new TranslationDto
                {
                    TranslationId = translationId,
                    QuestionnaireEntityId = questionId,
                    TranslationIndex = "1$",
                    Value = "test",
                    Type = TranslationType.OptionTitle
                },
                new TranslationDto
                {
                    TranslationId = translationId,
                    QuestionnaireEntityId = questionId,
                    TranslationIndex = "3",
                    Value = "Перевод 3й опции",
                    Type = TranslationType.OptionTitle
                },
            };

            var storage = Create.Storage.OptionsRepository(new SqliteInmemoryStorage<OptionView, int?>());

            storage.StoreOptionsForQuestion(questionnaireIdentity, questionId, question.Answers, translations);

            var filteredQuestionOptions = storage.GetFilteredQuestionOptions(questionnaireIdentity, questionId, null, null, translationId);

            var actual = filteredQuestionOptions.ToList();
            Assert.That(actual, Is.Not.Empty);
            Assert.That(actual.Count, Is.EqualTo(3));
            Assert.That(actual.First().Title, Is.EqualTo("test"));

            Assert.That(actual[2].Value, Is.EqualTo(3));
            Assert.That(actual[2].Title, Is.EqualTo("Перевод 3й опции"));
        }

        [Test]
        public void should_return_options_respecting_translation_with_no_extra()
        {
            var questionnaireIdentity = Create.Entity.QuestionnaireIdentity();

            var questionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var answerCodes = Enumerable.Range(0, 100).Reverse().ToArray();
            var options = new List<Answer>();

            foreach (var answerCode in answerCodes)
            {
                options.Add(Create.Entity.Answer(answerCode.ToString(), answerCode));
            }

            SingleQuestion question = Create.Entity.SingleQuestion(
                id: questionId,
                variable: "cat",
                options: options,
                isFilteredCombobox: true);

            var translationId = Guid.Parse("1AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            var translations = new List<TranslationDto>()
            {
                new TranslationDto()
                {
                    TranslationId = translationId,
                    QuestionnaireEntityId = questionId,
                    TranslationIndex = $"11$",
                    Value = 11.ToString() + "b",
                    Type = TranslationType.OptionTitle
                },
                new TranslationDto()
                {
                    TranslationId = translationId,
                    QuestionnaireEntityId = questionId,
                    TranslationIndex = $"21$",
                    Value = 21.ToString() + "a",
                    Type = TranslationType.OptionTitle
                }
            };

            var storage = Create.Storage.OptionsRepository(new SqliteInmemoryStorage<OptionView, int?>());

            storage.StoreOptionsForQuestion(questionnaireIdentity, questionId, question.Answers, translations);

            var filteredQuestionOptions = storage.GetFilteredQuestionOptions(questionnaireIdentity, questionId, null, null, translationId);

            var actual = filteredQuestionOptions.Select(x => x.Value).ToList();
            Assert.That(actual, Is.Not.Empty);
            Assert.That(actual.Count, Is.EqualTo(100));
            Assert.That(actual, Is.Ordered.Descending);

        }

        [Test]
        public void should_return_requested_option_by_title()
        {
            var questionnaireIdentity = Create.Entity.QuestionnaireIdentity();

            var questionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            var options = new List<Answer>();
            options.Add(Create.Entity.Answer(1.ToString(), 1));
            options.Add(Create.Entity.Answer(2.ToString(), 2));

            SingleQuestion question = Create.Entity.SingleQuestion(
                id: questionId,
                variable: "cat",
                options: options,
                isFilteredCombobox: true);

            var translationId = Guid.Parse("1AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            var optionTranslationValue = "test";

            var translations = new List<TranslationDto>()
            {
                new TranslationDto()
                {
                    TranslationId = translationId,
                    QuestionnaireEntityId = questionId,
                    TranslationIndex = 1.ToString(),
                    Value = optionTranslationValue,
                    Type = TranslationType.OptionTitle
                }
            };

            var storage = Create.Storage.OptionsRepository(new SqliteInmemoryStorage<OptionView, int?>());
            storage.StoreOptionsForQuestion(questionnaireIdentity, questionId, question.Answers, translations);
            var filteredQuestionOption = storage.GetQuestionOption(questionnaireIdentity, questionId, 1.ToString(), null, translationId);

            Assert.That(filteredQuestionOption, Is.Not.Null);
            Assert.That(filteredQuestionOption.Value, Is.EqualTo(1));
        }

        [Test]
        public void should_return_requested_option_by_value()
        {
            var questionnaireIdentity = Create.Entity.QuestionnaireIdentity();

            var questionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            var options = new List<Answer>();
            options.Add(Create.Entity.Answer(1.ToString(), 1));
            options.Add(Create.Entity.Answer(2.ToString(), 2));


            SingleQuestion question = Create.Entity.SingleQuestion(
                id: questionId,
                variable: "cat",
                options: options,
                isFilteredCombobox: true);

            var translationId = Guid.Parse("1AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            var optionTranslationValue = "test";

            var translations = new List<TranslationDto>()
            {
                new TranslationDto()
                {
                    TranslationId = translationId,
                    QuestionnaireEntityId = questionId,
                    TranslationIndex = 1.ToString(),
                    Value = optionTranslationValue,
                    Type = TranslationType.OptionTitle
                }
            };

            var storage = Create.Storage.OptionsRepository(new SqliteInmemoryStorage<OptionView, int?>());
            storage.StoreOptionsForQuestion(questionnaireIdentity, questionId, question.Answers, translations);

            // Assert
            var filteredQuestionOption = storage.GetQuestionOptionByValue(questionnaireIdentity, questionId, 1, null, translationId);
             
            CategoricalOption[] filteredOption1 = storage.GetOptionsByValues(questionnaireIdentity, questionId, new[] {1}, translationId);

            Assert.That(filteredOption1, Has.Length.EqualTo(1));
            Assert.That(filteredQuestionOption, Is.Not.Null);
            Assert.That(filteredQuestionOption.Value, Is.EqualTo(1));

            CategoricalOption[] filteredOption2 = storage.GetOptionsByValues(questionnaireIdentity, questionId, new[] {1, 2}, translationId);
            Assert.That(filteredOption2, Has.Length.EqualTo(2));
        }

        [Test]
        public void should_return_correct_option_when_contains_duplicate_titles()
        {
            var options = new List<Answer>();
            options.Add(Create.Entity.Answer("one", 1, 1));
            options.Add(Create.Entity.Answer("two", 2, 1));
            options.Add(Create.Entity.Answer("one", 3, 2));
            options.Add(Create.Entity.Answer("two", 4, 2));

            var questionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            SingleQuestion question = Create.Entity.SingleQuestion(
                id: questionId,
                variable: "cat",
                options: options,
                isFilteredCombobox: true);

            var questionnaireIdentity = Create.Entity.QuestionnaireIdentity();
            var storage = Create.Storage.OptionsRepository(new SqliteInmemoryStorage<OptionView, int?>());
            storage.StoreOptionsForQuestion(questionnaireIdentity, questionId, question.Answers, new List<TranslationDto>());

            // act
            var cascadingOption = storage.GetQuestionOption(questionnaireIdentity, questionId, "two", 2, null);

            // assert
            Assert.That(cascadingOption, Is.Not.Null);
            Assert.That(cascadingOption.Value, Is.EqualTo(4));
        }       
        
        [Test]
        public void should_keep_categories_options_order_after_saving_options()
        {
            var categoryId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var answerCodes = Enumerable.Range(0, 300).Reverse().Select(o => new CategoriesItem()
            {
                Id = o,
                Text = o.ToString()
            }).ToList();

            var storage = Create.Storage.OptionsRepository(new SqliteInmemoryStorage<OptionView, int?>());

            var questionnaireIdentity = Create.Entity.QuestionnaireIdentity();
            storage.StoreOptionsForCategory(questionnaireIdentity, categoryId, answerCodes, new List<TranslationDto>());

            var filteredQuestionOptions = storage.GetFilteredCategoriesOptions(questionnaireIdentity, categoryId, null, null, null);

            var actual = filteredQuestionOptions.Select(x => x.Value).ToList();
            Assert.That(actual, Is.Not.Empty);
            Assert.That(actual, Is.Ordered.Descending);
        }
        
        [Test]
        [SetCulture("sv-SE")]
        public void should_keep_categories_options_order_after_saving_options_in_Culture()
        {
            var categoryId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var answerCodes = Enumerable.Range(-5, 300).Reverse().Select(o => new CategoriesItem()
            {
                Id = o,
                Text = o.ToString()
            }).ToList();

            var storage = Create.Storage.OptionsRepository(new SqliteInmemoryStorage<OptionView, int?>());
            
            var questionnaireIdentity = Create.Entity.QuestionnaireIdentity();
            storage.StoreOptionsForCategory(questionnaireIdentity, categoryId, answerCodes, new List<TranslationDto>());

            var filteredQuestionOptions = storage.GetFilteredCategoriesOptions(questionnaireIdentity, categoryId, null, null, null);

            var actual = filteredQuestionOptions.Select(x => x.Value).ToList();
            Assert.That(actual, Is.Not.Empty);
            Assert.That(actual, Is.Ordered.Descending);
        }

        [Test]
        public void should_return_categories_options_respecting_translation()
        {
            var questionnaireIdentity = Create.Entity.QuestionnaireIdentity();
            var categoryId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            var options = new List<CategoriesItem>();
            options.Add(Create.Entity.CategoriesItem(1.ToString(), 1));
            options.Add(Create.Entity.CategoriesItem(2.ToString(), 2));

            var translationId = Guid.Parse("1AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            var optionTranslationValue = "test";

            var translations = new List<TranslationDto>()
            {
                new TranslationDto()
                {
                    TranslationId = translationId,
                    QuestionnaireEntityId = categoryId,
                    TranslationIndex = $"1$",
                    Value = optionTranslationValue,
                    Type = TranslationType.Categories
                }
            };

            var storage = Create.Storage.OptionsRepository(new SqliteInmemoryStorage<OptionView, int?>());

            storage.StoreOptionsForCategory(questionnaireIdentity, categoryId, options, translations);

            var filteredQuestionOptions = storage.GetFilteredCategoriesOptions(questionnaireIdentity, categoryId, null, null, translationId);

            var actual = filteredQuestionOptions.ToList();
            Assert.That(actual, Is.Not.Empty);
            Assert.That(actual.Count, Is.EqualTo(2));
            Assert.That(actual.First().Title, Is.EqualTo(optionTranslationValue));
        }

        [Test]
        public void should_return_categories_options_respecting_translation_with_no_extra()
        {
            var questionnaireIdentity = Create.Entity.QuestionnaireIdentity();

            var categoryId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var options = Enumerable.Range(0, 100)
                .Reverse()
                .Select(answerCode => Create.Entity.CategoriesItem(answerCode.ToString(), answerCode))
                .ToList();

            var translationId = Guid.Parse("1AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            var translations = new List<TranslationDto>()
            {
                new TranslationDto()
                {
                    TranslationId = translationId,
                    QuestionnaireEntityId = categoryId,
                    TranslationIndex = 11.ToString(),
                    Value = 11.ToString() + "b",
                    Type = TranslationType.OptionTitle
                },
                new TranslationDto()
                {
                    TranslationId = translationId,
                    QuestionnaireEntityId = categoryId,
                    TranslationIndex = 21.ToString(),
                    Value = 21.ToString() + "a",
                    Type = TranslationType.OptionTitle
                }
            };

            var storage = Create.Storage.OptionsRepository(new SqliteInmemoryStorage<OptionView, int?>());

            storage.StoreOptionsForCategory(questionnaireIdentity, categoryId, options, translations);

            var filteredQuestionOptions = storage.GetFilteredCategoriesOptions(questionnaireIdentity, categoryId, null, null, translationId);

            var actual = filteredQuestionOptions.Select(x => x.Value).ToList();
            Assert.That(actual, Is.Not.Empty);
            Assert.That(actual.Count, Is.EqualTo(100));
            Assert.That(actual, Is.Ordered.Descending);

        }

        [Test]
        public void should_return_requested_categories_option_by_title()
        {
            var questionnaireIdentity = Create.Entity.QuestionnaireIdentity();

            var categoryId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            var options = new List<CategoriesItem>();
            options.Add(Create.Entity.CategoriesItem(1.ToString(), 1));
            options.Add(Create.Entity.CategoriesItem(2.ToString(), 2));

            var translationId = Guid.Parse("1AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            var optionTranslationValue = "test";

            var translations = new List<TranslationDto>()
            {
                new TranslationDto()
                {
                    TranslationId = translationId,
                    QuestionnaireEntityId = categoryId,
                    TranslationIndex = 1.ToString(),
                    Value = optionTranslationValue,
                    Type = TranslationType.OptionTitle
                }
            };

            var storage = Create.Storage.OptionsRepository(new SqliteInmemoryStorage<OptionView, int?>());
            storage.StoreOptionsForCategory(questionnaireIdentity, categoryId, options, translations);
            var filteredQuestionOption = storage.GetCategoryOption(questionnaireIdentity, categoryId, 1.ToString(), null, translationId);

            Assert.That(filteredQuestionOption, Is.Not.Null);
            Assert.That(filteredQuestionOption.Value, Is.EqualTo(1));
        }

        [Test]
        public void should_return_requested_categories_option_by_value()
        {
            var questionnaireIdentity = Create.Entity.QuestionnaireIdentity();

            var categoryId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            var options = new List<CategoriesItem>();
            options.Add(Create.Entity.CategoriesItem(1.ToString(), 1));
            options.Add(Create.Entity.CategoriesItem(2.ToString(), 2));

            var translationId = Guid.Parse("1AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            var optionTranslationValue = "test";

            var translations = new List<TranslationDto>()
            {
                new TranslationDto()
                {
                    TranslationId = translationId,
                    QuestionnaireEntityId = categoryId,
                    TranslationIndex = 1.ToString(),
                    Value = optionTranslationValue,
                    Type = TranslationType.OptionTitle
                }
            };

            var storage = Create.Storage.OptionsRepository(new SqliteInmemoryStorage<OptionView, int?>());
            storage.StoreOptionsForCategory(questionnaireIdentity, categoryId, options, translations);

            // Assert
            var filteredQuestionOption = storage.GetCategoryOptionByValue(questionnaireIdentity, categoryId, 1, null, translationId);
             
            CategoricalOption[] filteredOption1 = storage.GetCategoryOptionsByValues(questionnaireIdentity, categoryId, new[] {1}, translationId);

            Assert.That(filteredOption1, Has.Length.EqualTo(1));
            Assert.That(filteredQuestionOption, Is.Not.Null);
            Assert.That(filteredQuestionOption.Value, Is.EqualTo(1));

            CategoricalOption[] filteredOption2 = storage.GetCategoryOptionsByValues(questionnaireIdentity, categoryId, new[] {1, 2}, translationId);
            Assert.That(filteredOption2, Has.Length.EqualTo(2));
        }

        [Test]
        public void should_return_correct_categories_option_when_contains_duplicate_titles()
        {
            var options = new List<CategoriesItem>();
            options.Add(Create.Entity.CategoriesItem("one", 1, 1));
            options.Add(Create.Entity.CategoriesItem("two", 2, 1));
            options.Add(Create.Entity.CategoriesItem("one", 3, 2));
            options.Add(Create.Entity.CategoriesItem("two", 4, 2));

            var categoryId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            var questionnaireIdentity = Create.Entity.QuestionnaireIdentity();
            var storage = Create.Storage.OptionsRepository(new SqliteInmemoryStorage<OptionView, int?>());
            storage.StoreOptionsForCategory(questionnaireIdentity, categoryId, options, new List<TranslationDto>());

            // act
            var cascadingOption = storage.GetCategoryOption(questionnaireIdentity, categoryId, "two", 2, null);

            // assert
            Assert.That(cascadingOption, Is.Not.Null);
            Assert.That(cascadingOption.Value, Is.EqualTo(4));
        }


        [Test]
        public void should_return_categories_options_respecting_translation_with_filter()
        {
            var questionnaireIdentity = Create.Entity.QuestionnaireIdentity();

            var categoryId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var options = Enumerable.Range(1, 100)
                .Reverse()
                .Select(answerCode => Create.Entity.CategoriesItem(answerCode.ToString(), answerCode))
                .ToList();

            var translationId = Guid.Parse("1AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            var translations = new List<TranslationDto>()
            {
                new TranslationDto()
                {
                    TranslationId = translationId,
                    QuestionnaireEntityId = categoryId,
                    TranslationIndex = $"11$",
                    Value = 11 + "b",
                    Type = TranslationType.Categories
                },
                new TranslationDto()
                {
                    TranslationId = translationId,
                    QuestionnaireEntityId = categoryId,
                    TranslationIndex = $"17$",
                    Value = 17 + "a",
                    Type = TranslationType.Categories
                }
            };

            var storage = Create.Storage.OptionsRepository(new SqliteInmemoryStorage<OptionView, int?>());

            storage.StoreOptionsForCategory(questionnaireIdentity, categoryId, options, translations);

            var filteredQuestionOptions = storage.GetFilteredCategoriesOptions(questionnaireIdentity, categoryId, null, "1", translationId).ToList();

            var actual = filteredQuestionOptions.Select(x => x.Value).ToList();
            Assert.That(actual, Is.Not.Empty);
            Assert.That(actual.Count, Is.EqualTo(20));
            Assert.That(actual, Is.Ordered.Descending);
            Assert.That(filteredQuestionOptions[18].Title, Is.EqualTo("10"));
            Assert.That(filteredQuestionOptions[17].Title, Is.EqualTo("11b"));
            Assert.That(filteredQuestionOptions[11].Title, Is.EqualTo("17a"));
        }
    }
}
