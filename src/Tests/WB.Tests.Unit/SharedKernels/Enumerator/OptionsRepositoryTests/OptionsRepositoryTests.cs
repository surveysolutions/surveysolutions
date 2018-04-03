﻿using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using NUnit.Framework;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Views;
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
            var answerCodes = Enumerable.Range(0, 300).Reverse().Select(Convert.ToDecimal).ToArray();

            SingleQuestion question = Create.Entity.SingleOptionQuestion(
                questionId: questionId,
                variable: "cat",
                answerCodes: answerCodes,
                isFilteredCombobox: true);

            var storage = new OptionsRepository(new SqliteInmemoryStorage<OptionView>());

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

            var storage = new OptionsRepository(new SqliteInmemoryStorage<OptionView>());

            storage.StoreOptionsForQuestion(questionnaireIdentity, questionId, question.Answers, translations);

            var filteredQuestionOptions = storage.GetFilteredQuestionOptions(questionnaireIdentity, questionId, null, null, translationId);

            var actual = filteredQuestionOptions.ToList();
            Assert.That(actual, Is.Not.Empty);
            Assert.That(actual.Count, Is.EqualTo(2));
            Assert.That(actual.First().Title, Is.EqualTo(optionTranslationValue));
        }

        [Test]
        public void should_return_options_respecting_translation_with_no_extra()
        {
            var questionnaireIdentity = Create.Entity.QuestionnaireIdentity();

            var questionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var answerCodes = Enumerable.Range(0, 100).Reverse().Select(Convert.ToDecimal).ToArray();
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
                    TranslationIndex = 11.ToString(),
                    Value = 11.ToString() + "b",
                    Type = TranslationType.OptionTitle
                },
                new TranslationDto()
                {
                    TranslationId = translationId,
                    QuestionnaireEntityId = questionId,
                    TranslationIndex = 21.ToString(),
                    Value = 21.ToString() + "a",
                    Type = TranslationType.OptionTitle
                }
            };

            var storage = new OptionsRepository(new SqliteInmemoryStorage<OptionView>());

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

            var storage = new OptionsRepository(new SqliteInmemoryStorage<OptionView>());
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

            var storage = new OptionsRepository(new SqliteInmemoryStorage<OptionView>());
            storage.StoreOptionsForQuestion(questionnaireIdentity, questionId, question.Answers, translations);
            var filteredQuestionOption = storage.GetQuestionOptionByValue(questionnaireIdentity, questionId, 1, translationId);

            Assert.That(filteredQuestionOption, Is.Not.Null);
            Assert.That(filteredQuestionOption.Value, Is.EqualTo(1));
        }

        [Test]
        public void should_return_correct_option_when_contains_duplidate_titles()
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
            var storage = new OptionsRepository(new SqliteInmemoryStorage<OptionView>());
            storage.StoreOptionsForQuestion(questionnaireIdentity, questionId, question.Answers, new List<TranslationDto>());

            // act
            var cascadingOption = storage.GetQuestionOption(questionnaireIdentity, questionId, "two", 2, null);

            // assert
            Assert.That(cascadingOption, Is.Not.Null);
            Assert.That(cascadingOption.Value, Is.EqualTo(4));
        }
    }
}