using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using NUnit.Framework;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Tests.Unit.SharedKernels.SurveyManagement;

namespace WB.Tests.Unit.SharedKernels.Enumerator.OptionsRepositoryTests
{
    [TestFixture(TestOf = typeof(OptionsRepository))]
    public class OptionsRepositoryTests
    {
        [Test]
        public async Task should_keep_options_order_after_saving_options()
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
            await storage.StoreOptionsForQuestionAsync(questionnaireIdentity, questionId, question.Answers, new List<TranslationDto>());

            var filteredQuestionOptions = storage.GetFilteredQuestionOptions(questionnaireIdentity, questionId, null, null, null);

            var actual = filteredQuestionOptions.Select(x => x.Value).ToList();
            Assert.That(actual, Is.Not.Empty);
            Assert.That(actual, Is.Ordered.Descending);
        }

        [Test]
        public async Task should_return_options_respecting_translation()
        {
            var questionnaireIdentity = Create.Entity.QuestionnaireIdentity();

            var questionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var answerCodes = Enumerable.Range(0, 300).Reverse().Select(Convert.ToDecimal).ToArray();

            var options = new List<Answer>();
            options.Add(Create.Entity.Answer(1.ToString(), 1));
            options.Add(Create.Entity.Answer(2.ToString(), 2));


            SingleQuestion question = Create.Entity.SingleQuestion(
                id: questionId,
                variable: "cat",
                options: options ,
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
            
            await storage.StoreOptionsForQuestionAsync(questionnaireIdentity, questionId, question.Answers, translations);

            var filteredQuestionOptions = storage.GetFilteredQuestionOptions(questionnaireIdentity, questionId, null, null, translationId);

            var actual = filteredQuestionOptions.ToList();
            Assert.That(actual, Is.Not.Empty);
            Assert.That(actual.Count, Is.EqualTo(2));
            Assert.That(actual.First().Title, Is.EqualTo(optionTranslationValue));
        }
    }
}