using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Search;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireSearchStorageTests
{
    [TestOf(typeof(QuestionnaireSearchStorage))]
    internal class CreateTextSearchQueryTests
    {
        [TestCase("<<<")]
        [TestCase(">>>")]
        [TestCase("&|!():*")]
        [TestCase("' \"")]
        [TestCase("\\")]
        public void when_query_contains_only_tsquery_special_characters_should_return_empty_query(string query)
        {
            var result = QuestionnaireSearchStorage.CreateTextSearchQuery(query);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void when_single_word_query_should_produce_prefix_search()
        {
            var result = QuestionnaireSearchStorage.CreateTextSearchQuery("house");

            Assert.That(result, Is.EqualTo("house:*"));
        }

        [Test]
        public void when_single_word_query_with_special_characters_should_strip_them_and_produce_prefix_search()
        {
            var result = QuestionnaireSearchStorage.CreateTextSearchQuery("<<house>>");

            Assert.That(result, Is.EqualTo("house:*"));
        }

        [TestCase("car dog")]
        [TestCase("car\tdog")]
        [TestCase("car\ndog")]
        public void when_multi_word_query_should_join_words_with_logical_and(string query)
        {
            var result = QuestionnaireSearchStorage.CreateTextSearchQuery(query);

            Assert.That(result, Is.EqualTo("car & dog"));
        }

        [Test]
        public void when_multi_word_query_with_special_characters_should_strip_them_and_join_with_logical_and()
        {
            var result = QuestionnaireSearchStorage.CreateTextSearchQuery("car <<< dog | ()");

            Assert.That(result, Is.EqualTo("car & dog"));
        }

        [Test]
        public void when_query_is_null_should_return_null()
        {
            var result = QuestionnaireSearchStorage.CreateTextSearchQuery(null);

            Assert.That(result, Is.Null);
        }
    }
}
