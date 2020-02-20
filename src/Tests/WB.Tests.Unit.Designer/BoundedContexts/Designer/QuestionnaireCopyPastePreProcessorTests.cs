using System;
using System.Linq;
using Main.Core.Entities.Composite;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Implementation.Services.QuestionnairePostProcessors;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer
{
    [TestOf(typeof(CopyPastePreProcessor))]
    internal class QuestionnaireCopyPastePreProcessorTests
    {
        [Test]
        public void when_process_past_after_and_categorical_question_with_reusable_categories_when_reusable_categories_should_be_copied_from_source_to_target_questionnaire()
        {
            // arrange
            var categoriesId = Id.g2;
            var categoricalQuestionId = Id.g3;
            var sourceQuestionnaireId = Id.g1;
            var targetQuestionnaireId = Id.g4;

            var sourceQuestionnaire = Create.QuestionnaireDocument(sourceQuestionnaireId,
                Create.SingleOptionQuestion(categoricalQuestionId, categoriesId: categoriesId));
            sourceQuestionnaire.Categories.Add(Create.Categories(categoriesId, "cat"));

            var db = Create.InMemoryDbContext();
            db.CategoriesInstances.AddRange(
                new[]
                {
                    Create.CategoriesInstance(sourceQuestionnaireId, categoriesId, 1),
                    Create.CategoriesInstance(sourceQuestionnaireId, categoriesId, 2)
                });
            db.SaveChanges();
            
            var categoriesService = Create.CategoriesService(db);
            var processor = Create.CopyPastePreProcessor(categoriesService);
            // act
            var pasteAfter = new PasteAfter(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                sourceQuestionnaireId,
                categoricalQuestionId,
                Guid.NewGuid()) {SourceDocument = sourceQuestionnaire};

            processor.Process(Create.Questionnaire(Guid.NewGuid(), Create.QuestionnaireDocument(targetQuestionnaireId)), pasteAfter);
            db.SaveChanges();
            // assert
            var sourceCategories = categoriesService.GetCategoriesById(sourceQuestionnaireId, categoriesId).ToArray();
            var targetCategories = categoriesService.GetCategoriesById(targetQuestionnaireId, categoriesId).ToArray();
            
            Assert.That(sourceCategories, Has.Exactly(2).Items);
            Assert.That(targetCategories, Has.Exactly(2).Items);
            Assert.That(sourceCategories, Is.EqualTo(targetCategories));
        }

        [Test]
        public void when_process_past_into_and_categorical_question_with_reusable_categories_when_reusable_categories_should_be_copied_from_source_to_target_questionnaire()
        {
            // arrange
            var categoriesId = Id.g2;
            var categoricalQuestionId = Id.g3;
            var sourceQuestionnaireId = Id.g1;
            var targetQuestionnaireId = Id.g4;

            var sourceQuestionnaire = Create.QuestionnaireDocument(sourceQuestionnaireId,
                Create.SingleOptionQuestion(categoricalQuestionId, categoriesId: categoriesId));
            sourceQuestionnaire.Categories.Add(Create.Categories(categoriesId, "cat"));

            var db = Create.InMemoryDbContext();
            db.CategoriesInstances.AddRange(
                new[]
                {
                    Create.CategoriesInstance(sourceQuestionnaireId, categoriesId, 1),
                    Create.CategoriesInstance(sourceQuestionnaireId, categoriesId, 2)
                });
            db.SaveChanges();
            
            var categoriesService = Create.CategoriesService(db);
            var processor = Create.CopyPastePreProcessor(categoriesService);
            // act
            var pasteInto = new PasteInto(
                Guid.NewGuid(),
                Guid.NewGuid(),
                sourceQuestionnaireId,
                categoricalQuestionId,
                Guid.NewGuid(), 
                Guid.NewGuid()) {SourceDocument = sourceQuestionnaire};

            processor.Process(Create.Questionnaire(Guid.NewGuid(), Create.QuestionnaireDocument(targetQuestionnaireId)), pasteInto);
            db.SaveChanges();
            // assert
            var sourceCategories = categoriesService.GetCategoriesById(sourceQuestionnaireId, categoriesId).ToArray();
            var targetCategories = categoriesService.GetCategoriesById(targetQuestionnaireId, categoriesId).ToArray();
            
            Assert.That(sourceCategories, Has.Exactly(2).Items);
            Assert.That(targetCategories, Has.Exactly(2).Items);
            Assert.That(sourceCategories, Is.EqualTo(targetCategories));
        }

        [Test]
        public void when_process_past_after_and_group_with_categorical_questions_with_reusable_categories_when_reusable_categories_should_be_copied_from_source_to_target_questionnaire()
        {
            // arrange
            var categories1Id = Id.g2;
            var categories2Id = Id.g5;
            var categoricalQuestion1Id = Id.g3;
            var categoricalQuestion2Id = Id.g6;
            var groupId = Id.g7;
            var rosterId = Id.g8;
            var sourceQuestionnaireId = Id.g1;
            var targetQuestionnaireId = Id.g4;

            var sourceQuestionnaire = Create.QuestionnaireDocument(sourceQuestionnaireId,
                Create.Group(groupId, children: new IComposite[]
                {
                    Create.SingleOptionQuestion(categoricalQuestion1Id, categoriesId: categories1Id),
                    Create.FixedRoster(rosterId,
                        children: new[]
                        {
                            Create.SingleOptionQuestion(categoricalQuestion2Id, categoriesId: categories2Id)

                        })
                }));
            sourceQuestionnaire.Categories.Add(Create.Categories(categories1Id, "cat"));

            var db = Create.InMemoryDbContext();
            db.CategoriesInstances.AddRange(
                new[]
                {
                    Create.CategoriesInstance(sourceQuestionnaireId, categories1Id, 1),
                    Create.CategoriesInstance(sourceQuestionnaireId, categories1Id, 2),
                    Create.CategoriesInstance(sourceQuestionnaireId, categories2Id, 100),
                    Create.CategoriesInstance(sourceQuestionnaireId, categories2Id, 200)
                });
            db.SaveChanges();
            
            var categoriesService = Create.CategoriesService(db);
            var processor = Create.CopyPastePreProcessor(categoriesService);
            // act
            var pasteAfter = new PasteAfter(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                sourceQuestionnaireId,
                groupId,
                Guid.NewGuid()) {SourceDocument = sourceQuestionnaire};

            processor.Process(Create.Questionnaire(Guid.NewGuid(), Create.QuestionnaireDocument(targetQuestionnaireId)), pasteAfter);
            db.SaveChanges();
            // assert
            var sourceCategories1 = categoriesService.GetCategoriesById(sourceQuestionnaireId, categories1Id).ToArray();
            var targetCategories1 = categoriesService.GetCategoriesById(targetQuestionnaireId, categories1Id).ToArray();

            var sourceCategories2 = categoriesService.GetCategoriesById(sourceQuestionnaireId, categories2Id).ToArray();
            var targetCategories2 = categoriesService.GetCategoriesById(targetQuestionnaireId, categories2Id).ToArray();
            
            Assert.That(sourceCategories1, Has.Exactly(2).Items);
            Assert.That(targetCategories1, Has.Exactly(2).Items);
            Assert.That(sourceCategories1, Is.EqualTo(targetCategories1));

            Assert.That(sourceCategories2, Has.Exactly(2).Items);
            Assert.That(targetCategories2, Has.Exactly(2).Items);
            Assert.That(sourceCategories2, Is.EqualTo(targetCategories2));
        }

        [Test]
        public void when_process_past_into_and_group_with_categorical_questions_with_reusable_categories_when_reusable_categories_should_be_copied_from_source_to_target_questionnaire()
        {
            // arrange
            var categories1Id = Id.g2;
            var categories2Id = Id.g5;
            var categoricalQuestion1Id = Id.g3;
            var categoricalQuestion2Id = Id.g6;
            var groupId = Id.g7;
            var rosterId = Id.g8;
            var sourceQuestionnaireId = Id.g1;
            var targetQuestionnaireId = Id.g4;

            var sourceQuestionnaire = Create.QuestionnaireDocument(sourceQuestionnaireId,
                Create.Group(groupId, children: new IComposite[]
                {
                    Create.SingleOptionQuestion(categoricalQuestion1Id, categoriesId: categories1Id),
                    Create.FixedRoster(rosterId,
                        children: new[]
                        {
                            Create.SingleOptionQuestion(categoricalQuestion2Id, categoriesId: categories2Id)

                        })
                }));
            sourceQuestionnaire.Categories.Add(Create.Categories(categories1Id, "cat"));

            var db = Create.InMemoryDbContext();
            db.CategoriesInstances.AddRange(
                new[]
                {
                    Create.CategoriesInstance(sourceQuestionnaireId, categories1Id, 1),
                    Create.CategoriesInstance(sourceQuestionnaireId, categories1Id, 2),
                    Create.CategoriesInstance(sourceQuestionnaireId, categories2Id, 100),
                    Create.CategoriesInstance(sourceQuestionnaireId, categories2Id, 200)
                });
            db.SaveChanges();
            
            var categoriesService = Create.CategoriesService(db);
            var processor = Create.CopyPastePreProcessor(categoriesService);
            // act
            var pasteInto = new PasteInto(
                Guid.NewGuid(),
                Guid.NewGuid(),
                sourceQuestionnaireId,
                groupId,
                Guid.NewGuid(), 
                Guid.NewGuid()) {SourceDocument = sourceQuestionnaire};

            processor.Process(Create.Questionnaire(Guid.NewGuid(), Create.QuestionnaireDocument(targetQuestionnaireId)), pasteInto);
            db.SaveChanges();
            // assert
            var sourceCategories1 = categoriesService.GetCategoriesById(sourceQuestionnaireId, categories1Id).ToArray();
            var targetCategories1 = categoriesService.GetCategoriesById(targetQuestionnaireId, categories1Id).ToArray();

            var sourceCategories2 = categoriesService.GetCategoriesById(sourceQuestionnaireId, categories2Id).ToArray();
            var targetCategories2 = categoriesService.GetCategoriesById(targetQuestionnaireId, categories2Id).ToArray();
            
            Assert.That(sourceCategories1, Has.Exactly(2).Items);
            Assert.That(targetCategories1, Has.Exactly(2).Items);
            Assert.That(sourceCategories1, Is.EqualTo(targetCategories1));

            Assert.That(sourceCategories2, Has.Exactly(2).Items);
            Assert.That(targetCategories2, Has.Exactly(2).Items);
            Assert.That(sourceCategories2, Is.EqualTo(targetCategories2));
        }

        [Test]
        public void when_process_past_after_and_categorical_question_with_reusable_categories_paste_2_times_when_reusable_categories_should_not_be_copied_second_time()
        {
            // arrange
            var categoriesId = Id.g2;
            var categoricalQuestionId = Id.g3;
            var sourceQuestionnaireId = Id.g1;
            var targetQuestionnaireId = Id.g4;

            var sourceQuestionnaire = Create.QuestionnaireDocument(sourceQuestionnaireId,
                Create.SingleOptionQuestion(categoricalQuestionId, categoriesId: categoriesId));
            sourceQuestionnaire.Categories.Add(Create.Categories(categoriesId, "cat"));

            var db = Create.InMemoryDbContext();
            db.CategoriesInstances.AddRange(
                new[]
                {
                    Create.CategoriesInstance(sourceQuestionnaireId, categoriesId, 1),
                    Create.CategoriesInstance(sourceQuestionnaireId, categoriesId, 2)
                });
            db.SaveChanges();
            
            var categoriesService = Create.CategoriesService(db);
            var processor = Create.CopyPastePreProcessor(categoriesService);
            // act
            var pasteAfter = new PasteAfter(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                sourceQuestionnaireId,
                categoricalQuestionId,
                Guid.NewGuid()) {SourceDocument = sourceQuestionnaire};

            processor.Process(Create.Questionnaire(Guid.NewGuid(), Create.QuestionnaireDocument(targetQuestionnaireId)), pasteAfter);
            db.SaveChanges();
            processor.Process(Create.Questionnaire(Guid.NewGuid(), Create.QuestionnaireDocument(targetQuestionnaireId)), pasteAfter);
            db.SaveChanges();
            // assert
            var sourceCategories = categoriesService.GetCategoriesById(sourceQuestionnaireId, categoriesId).ToArray();
            var targetCategories = categoriesService.GetCategoriesById(targetQuestionnaireId, categoriesId).ToArray();
            
            Assert.That(sourceCategories, Has.Exactly(2).Items);
            Assert.That(targetCategories, Has.Exactly(2).Items);
            Assert.That(sourceCategories, Is.EqualTo(targetCategories));
        }

        [Test]
        public void when_process_past_into_and_categorical_question_with_reusable_categories_2_times_when_reusable_categories_should_not_be_copied_second_time()
        {
            // arrange
            var categoriesId = Id.g2;
            var categoricalQuestionId = Id.g3;
            var sourceQuestionnaireId = Id.g1;
            var targetQuestionnaireId = Id.g4;

            var sourceQuestionnaire = Create.QuestionnaireDocument(sourceQuestionnaireId,
                Create.SingleOptionQuestion(categoricalQuestionId, categoriesId: categoriesId));
            sourceQuestionnaire.Categories.Add(Create.Categories(categoriesId, "cat"));

            var db = Create.InMemoryDbContext();
            db.CategoriesInstances.AddRange(
                new[]
                {
                    Create.CategoriesInstance(sourceQuestionnaireId, categoriesId, 1),
                    Create.CategoriesInstance(sourceQuestionnaireId, categoriesId, 2)
                });
            db.SaveChanges();
            
            var categoriesService = Create.CategoriesService(db);
            var processor = Create.CopyPastePreProcessor(categoriesService);
            // act
            var pasteInto = new PasteInto(
                Guid.NewGuid(),
                Guid.NewGuid(),
                sourceQuestionnaireId,
                categoricalQuestionId,
                Guid.NewGuid(), 
                Guid.NewGuid()) {SourceDocument = sourceQuestionnaire};

            processor.Process(Create.Questionnaire(Guid.NewGuid(), Create.QuestionnaireDocument(targetQuestionnaireId)), pasteInto);
            db.SaveChanges();
            processor.Process(Create.Questionnaire(Guid.NewGuid(), Create.QuestionnaireDocument(targetQuestionnaireId)), pasteInto);
            db.SaveChanges();
            // assert
            var sourceCategories = categoriesService.GetCategoriesById(sourceQuestionnaireId, categoriesId).ToArray();
            var targetCategories = categoriesService.GetCategoriesById(targetQuestionnaireId, categoriesId).ToArray();
            
            Assert.That(sourceCategories, Has.Exactly(2).Items);
            Assert.That(targetCategories, Has.Exactly(2).Items);
            Assert.That(sourceCategories, Is.EqualTo(targetCategories));
        }
    }
}
