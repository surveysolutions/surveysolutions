using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Search;

namespace WB.Tests.Integration.Designer
{
    [TestOf(typeof(QuestionnaireSearchStorage))]
    [NonParallelizable]
    internal class QuestionnaireSearchStorageTests : IntegrationTest
    {
        [Test]
        public void when_remove_entity_data_from_search_storage_should_dont_find_entity_by_text()
        {
            var questionId = Guid.NewGuid();
            var questionnaireId = Guid.NewGuid();
            var questionnaireTitle = "q-title";

            RunActionInScope(sl =>
            {
                var dbContext = sl.GetInstance<DesignerDbContext>();
                dbContext.Questionnaires.Add(Create.Questionnaire.ListViewItem(questionnaireId, questionnaireTitle));
                dbContext.SaveChanges();

                var searchStorage = sl.GetInstance<IQuestionnaireSearchStorage>();
                searchStorage.AddOrUpdateEntity(questionnaireId, 
                    new TextQuestion() { QuestionText = "question text", PublicKey = questionId });
            });

            RunActionInScope(sl =>
            {
                var searchStorage = sl.GetInstance<IQuestionnaireSearchStorage>();
                searchStorage.Remove(questionnaireId, questionId);
            });

            RunActionInScope(sl =>
            {
                var searchStorage = sl.GetInstance<IQuestionnaireSearchStorage>();
                var searchResult = searchStorage.Search(new SearchInput() { Query = "texts", PageSize = 20 });

                Assert.That(searchResult.Items.Count, Is.EqualTo(0));
            });
        }

        [Test]
        public void when_remove_questionnaire_data_from_search_storage_should_dont_find_entity_by_text()
        {
            var questionId = Guid.NewGuid();
            var questionnaireId = Guid.NewGuid();
            var questionnaireTitle = "q-title";

            RunActionInScope(sl =>
            {
                var dbContext = sl.GetInstance<DesignerDbContext>();
                dbContext.Questionnaires.Add(Create.Questionnaire.ListViewItem(questionnaireId, questionnaireTitle));
                dbContext.SaveChanges();

                var searchStorage = sl.GetInstance<IQuestionnaireSearchStorage>();
                searchStorage.AddOrUpdateEntity(questionnaireId, 
                    new TextQuestion() {QuestionText = "question text", PublicKey = questionId });
            });

            RunActionInScope(sl =>
            {
                var searchStorage = sl.GetInstance<IQuestionnaireSearchStorage>();
                searchStorage.RemoveAllEntities(questionnaireId);
            });

            RunActionInScope(sl =>
            {
                var searchStorage = sl.GetInstance<IQuestionnaireSearchStorage>();
                var searchResult = searchStorage.Search(new SearchInput() { Query = "texts", PageSize = 20 });

                Assert.That(searchResult.Items.Count, Is.EqualTo(0));
            });
        }

        [Test]
        public void when_search_use_folder_in_search_request_should_dont_found_elements_because_folder_with_data_upper_in_folder_hierarchy()
        {
            var questionId = Guid.NewGuid();
            var questionnaireId = Guid.NewGuid();
            var questionnaireTitle = "q-title";

            var parentFolderId = Guid.NewGuid();
            var folderId = Guid.NewGuid();
            var folderTitle = "it is folder";

            RunActionInScope(sl =>
            {
                var dbContext = sl.GetInstance<DesignerDbContext>();
                dbContext.QuestionnaireFolders.Add(Create.Questionnaire.ListViewFolder(parentFolderId, "parent folder", null, 1));
                dbContext.QuestionnaireFolders.Add(Create.Questionnaire.ListViewFolder(folderId, folderTitle, parentFolderId, 2, path: $"\\{parentFolderId}\\"));
                dbContext.Questionnaires.Add(Create.Questionnaire.ListViewItem(questionnaireId, questionnaireTitle, folderId: parentFolderId));
                dbContext.SaveChanges();

                var searchStorage = sl.GetInstance<IQuestionnaireSearchStorage>();
                searchStorage.AddOrUpdateEntity(questionnaireId, 
                    new TextQuestion() {QuestionText = "test car dog question text", PublicKey = questionId });
            });

            RunActionInScope(sl =>
            {
                var searchStorage = sl.GetInstance<IQuestionnaireSearchStorage>();
                var searchResult = searchStorage.Search(new SearchInput() { Query = "cars dogs", FolderId = folderId });

                Assert.That(searchResult.Items.Count, Is.EqualTo(0));
            });
        }

        [Test]
        public void when_search_use_folder_in_search_request_for_inner_folders_should_found_elements_because_folder_with_data_down_in_folder_hierarchy()
        {
            var questionId = Guid.NewGuid();
            var questionnaireId = Guid.NewGuid();
            var questionnaireTitle = "q-title";

            var parentFolderId = Guid.NewGuid();
            var folderId = Guid.NewGuid();
            var folderTitle = "it is folder";

            RunActionInScope(sl =>
            {
                var dbContext = sl.GetInstance<DesignerDbContext>();
                dbContext.QuestionnaireFolders.Add(Create.Questionnaire.ListViewFolder(parentFolderId, "parent folder", null, 1));
                dbContext.QuestionnaireFolders.Add(Create.Questionnaire.ListViewFolder(folderId, folderTitle, parentFolderId, 2, path: $"\\{parentFolderId}\\"));
                dbContext.Questionnaires.Add(Create.Questionnaire.ListViewItem(questionnaireId, questionnaireTitle, folderId: folderId));
                dbContext.SaveChanges();

                var searchStorage = sl.GetInstance<IQuestionnaireSearchStorage>();
                searchStorage.AddOrUpdateEntity(questionnaireId, 
                    new TextQuestion() {QuestionText = "test car dog question text", PublicKey = questionId });
                
            });

            RunActionInScope(sl =>
            {
                var searchStorage = sl.GetInstance<IQuestionnaireSearchStorage>();
                var searchResult = searchStorage.Search(new SearchInput() { Query = "cars dogs", FolderId = parentFolderId });

                Assert.That(searchResult.Items.Count, Is.EqualTo(1));
                Assert.That(searchResult.Items.Single().FolderName, Is.EqualTo(folderTitle));
            });
        }

        [Test]
        public void when_search_with_many_words_in_query_should_search_entity_by_text()
        {
            var questionId = Guid.NewGuid();
            var questionnaireId = Guid.NewGuid();
            var questionnaireTitle = "q-title";

            RunActionInScope(sl =>
            {
                var dbContext = sl.GetInstance<DesignerDbContext>();

                dbContext.Questionnaires.Add(Create.Questionnaire.ListViewItem(questionnaireId, questionnaireTitle));
                dbContext.SaveChanges();

                var searchStorage = sl.GetInstance<IQuestionnaireSearchStorage>();
                searchStorage.AddOrUpdateEntity(questionnaireId, Create.Questionnaire.TextQuestion(questionId, text: "test car dog question text"));
            });

            SearchResult searchResult = null;

            RunActionInScope(sl =>
            {
                var searchStorage = sl.GetInstance<IQuestionnaireSearchStorage>();
                searchResult = searchStorage.Search(new SearchInput() { Query = "cars dogs", PageSize = 20 });
            });

            Assert.That(searchResult.Items.Count, Is.EqualTo(1));
            Assert.That(searchResult.Items.Single().EntityId, Is.EqualTo(questionId));
            Assert.That(searchResult.Items.Single().EntityType, Is.EqualTo(QuestionType.Text.ToString()));
            Assert.That(searchResult.Items.Single().QuestionnaireId, Is.EqualTo(questionnaireId));
            Assert.That(searchResult.Items.Single().QuestionnaireTitle, Is.EqualTo(questionnaireTitle));
        }

        [Test]
        public void when_store_entities_with_title_equals_to_search_query_should_search_entities_by_text()
        {
            var questionnaireId = Guid.NewGuid();
            var questionnaireTitle = "q-title";

            RunActionInScope(sl =>
            {
                var dbContext = sl.GetInstance<DesignerDbContext>();

                dbContext.Questionnaires.Add(Create.Questionnaire.ListViewItem(questionnaireId, questionnaireTitle));
                dbContext.SaveChanges();

                var searchStorage = sl.GetInstance<IQuestionnaireSearchStorage>();
                searchStorage.AddOrUpdateEntity(questionnaireId, 
                    new TextQuestion() {QuestionText = "question text", PublicKey = Guid.NewGuid() });
                searchStorage.AddOrUpdateEntity(questionnaireId, 
                    new TextQuestion() {QuestionText = "text from tales", PublicKey = Guid.NewGuid() });
            });

            SearchResult searchResult = null;

            RunActionInScope(sl =>
            {
                var searchStorage = sl.GetInstance<IQuestionnaireSearchStorage>();
                searchResult = searchStorage.Search(new SearchInput() { Query = "texts", PageSize = 20 });
            });

            Assert.That(searchResult.Items.Count, Is.EqualTo(2));
            Assert.That(searchResult.Items.First().Title, Is.EqualTo("text from tales"));
            Assert.That(searchResult.Items.Last().Title, Is.EqualTo("question text"));
        }

        [Test]
        public void when_store_entity_with_title_equals_to_search_query_should_search_entity_by_text()
        {
            var questionId = Guid.NewGuid();
            var questionnaireId = Guid.NewGuid();
            var questionnaireTitle = "q-title";

            RunActionInScope(sl =>
            {
                var dbContext = sl.GetInstance<DesignerDbContext>();

                dbContext.Questionnaires.Add(Create.Questionnaire.ListViewItem(questionnaireId, questionnaireTitle));
                dbContext.SaveChanges();

                var searchStorage = sl.GetInstance<IQuestionnaireSearchStorage>();
                searchStorage.AddOrUpdateEntity(questionnaireId, Create.Questionnaire.TextQuestion(questionId, text: "question text"));
            });

            SearchResult searchResult = null;

            RunActionInScope(sl =>
            {
                var searchStorage = sl.GetInstance<IQuestionnaireSearchStorage>();
                searchResult = searchStorage.Search(new SearchInput() { Query = "texts", PageSize = 20 });
            });

            Assert.That(searchResult.Items.Count, Is.EqualTo(1));
            Assert.That(searchResult.Items.Single().EntityId, Is.EqualTo(questionId));
            Assert.That(searchResult.Items.Single().EntityType, Is.EqualTo(QuestionType.Text.ToString()));
            Assert.That(searchResult.Items.Single().QuestionnaireId, Is.EqualTo(questionnaireId));
            Assert.That(searchResult.Items.Single().QuestionnaireTitle, Is.EqualTo(questionnaireTitle));
        }

    }
}
