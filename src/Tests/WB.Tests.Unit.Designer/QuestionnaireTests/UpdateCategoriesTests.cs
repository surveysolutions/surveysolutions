using System;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Tests.Abc;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.QuestionnaireTests
{
    [TestFixture]
    internal class UpdateCategoriesTests : QuestionnaireTestsContext
    {
        [Test]
        public void When_DeleteCategories_Then_categories_should_be_deleted_from_categories_collections_and_source_of_dependent_categorical_questions_should_be_changed_to_user_defined()
        {
            // arrange
            Guid categoriesId = Id.g1;
            Guid newCategoriesId = Id.g10;
            Guid responsibleId = Id.g2;
            Guid singleQuestionId = Id.g3;
            Guid groupId = Id.g4;
            Guid multiQuestionId = Id.g5;

            Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(responsibleId, groupId: groupId);
            questionnaire.AddOrUpdateCategories(Create.AddOrUpdateCategories(questionnaire.Id, responsibleId, categoriesId));
            questionnaire.AddSingleOptionQuestion(singleQuestionId, groupId, responsibleId, categoriesId: categoriesId);
            questionnaire.AddMultiOptionQuestion(multiQuestionId, groupId, responsibleId, categoriesId: categoriesId);

            // act
            questionnaire.AddOrUpdateCategories(Create.AddOrUpdateCategories(questionnaire.Id, responsibleId, newCategoriesId, "newCategory", categoriesId));

            // assert
            Assert.That(questionnaire.QuestionnaireDocument.Categories.Find(x => x.Id == categoriesId), Is.Null);
            Assert.That(questionnaire.QuestionnaireDocument.Categories.Find(x => x.Id == newCategoriesId), Is.Not.Null);
            Assert.That(questionnaire.QuestionnaireDocument.Find<ICategoricalQuestion>(singleQuestionId).CategoriesId, Is.EqualTo(Id.g10));
            Assert.That(questionnaire.QuestionnaireDocument.Find<ICategoricalQuestion>(multiQuestionId).CategoriesId, Is.EqualTo(Id.g10));
        }
    }
}
