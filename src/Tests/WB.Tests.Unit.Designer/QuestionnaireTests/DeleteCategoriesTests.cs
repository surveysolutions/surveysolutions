using System;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    [TestFixture]
    internal class DeleteCategoriesTests : QuestionnaireTestsContext
    {
        [Test]
        public void When_DeleteCategories_Then_categories_should_be_deleted_from_categories_collections_and_source_of_dependent_categorical_questions_should_be_changed_to_user_defined()
        {
            // arrange
            Guid categoriesId = Id.g1;
            Guid responsibleId = Id.g2;
            Guid singleQuestionId = Id.g3;
            Guid groupId = Id.g4;
            Guid multiQuestionId = Id.g5;

            Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(responsibleId, groupId: groupId);
            questionnaire.AddOrUpdateCategories(Create.AddOrUpdateCategories(questionnaire.Id, responsibleId, categoriesId));
            questionnaire.AddSingleOptionQuestion(singleQuestionId, groupId, responsibleId, categoriesId: categoriesId);
            questionnaire.AddMultiOptionQuestion(multiQuestionId, groupId, responsibleId, categoriesId: categoriesId);

            // act
            questionnaire.DeleteCategories(Create.DeleteCategories(questionnaire.Id, responsibleId, categoriesId));

            // assert
            Assert.That(questionnaire.QuestionnaireDocument.Categories.Find(x => x.Id == categoriesId), Is.Null);
            Assert.That(questionnaire.QuestionnaireDocument.Find<ICategoricalQuestion>(singleQuestionId).CategoriesId, Is.Null);
            Assert.That(questionnaire.QuestionnaireDocument.Find<ICategoricalQuestion>(multiQuestionId).CategoriesId, Is.Null);
        }

        [Test]
        public void When_DeleteCategories_And_User_Doesnot_Have_Permissions_For_Edit_Questionnaire_Then_DomainException_should_be_thrown()
        {
            // arrange
            Guid categoriesId = Id.g1;
            Guid responsibleId = Id.g2;

            Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(responsibleId);
            questionnaire.AddOrUpdateCategories(Create.AddOrUpdateCategories(questionnaire.Id, responsibleId, categoriesId));

            // act
            TestDelegate act = () => questionnaire.DeleteCategories(Create.DeleteCategories(questionnaire.Id, Guid.NewGuid(), categoriesId));
            // assert
            var domainException = Assert.Throws<QuestionnaireException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.DoesNotHavePermissionsForEdit));
        }
    }
}
