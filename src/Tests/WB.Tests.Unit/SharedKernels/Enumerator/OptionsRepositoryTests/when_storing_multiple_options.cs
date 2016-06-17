using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Views;
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
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
               Create.Entity.SingleOptionQuestion(
                   questionId: questionId,
                   variable: "cat",
                   answerCodes: answerCodes,
                   isFilteredCombobox: true));

            var storage = new OptionsRepository(new SqliteInmemoryStorage<OptionView>());

            var questionnaireIdentity = Create.Entity.QuestionnaireIdentity();
            await storage.StoreQuestionOptionsForQuestionnaireAsync(questionnaireIdentity, questionnaire);

            var filteredQuestionOptions = storage.GetFilteredQuestionOptions(questionnaireIdentity, questionId, null, null);

            var actual = filteredQuestionOptions.Select(x => x.Value).ToList();
            Assert.That(actual, Is.Not.Empty);
            Assert.That(actual, Is.Ordered.Descending);
        }
    }
}