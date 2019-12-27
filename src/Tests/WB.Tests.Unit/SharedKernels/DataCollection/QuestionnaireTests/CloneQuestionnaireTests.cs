using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Implementation.Aggregates;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Questionnaire.Categories;
using WB.Infrastructure.Native.Questionnaire;
using WB.Infrastructure.Native.Questionnaire.Impl;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.QuestionnaireTests
{
    [TestOf(typeof(Questionnaire))]
    internal class CloneQuestionnaireTests : QuestionnaireTestsContext
    {
        [Test]
        public void when_cloning_questionnaire_with_categories()
        {
            int targetQuestionnaireVersion = 5;
            Guid questionnaireId = Id.g1;
            Guid categoriesId = Id.g2;
            var sourceQuestionnaireId = Create.Entity.QuestionnaireIdentity(questionnaireId, 4);

            
            var categoriesPlainStorage = Create.Storage.InMemoryPlainStorage<ReusableCategoricalOptions>();
            var categoriesStorage = new ReusableCategoriesStorage(categoriesPlainStorage);

            var sourceCategories = new List<CategoriesItem>(new []
            {
                Create.Entity.CategoriesItem("opt 1", 1, 1),
                Create.Entity.CategoriesItem("opt 2", 2, 1)
            });

            categoriesStorage.Store(sourceQuestionnaireId, categoriesId, sourceCategories);

            var questionnaireDocument = Create.Entity.QuestionnaireDocument();
            questionnaireDocument.PublicKey = sourceQuestionnaireId.QuestionnaireId;

            var plainQuestionnaireRepositoryMock = 
                Mock.Get(Mock.Of<IQuestionnaireStorage>(_ => 
                    _.GetQuestionnaireDocument(sourceQuestionnaireId.QuestionnaireId, sourceQuestionnaireId.Version) == questionnaireDocument));

            IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItemStorage
                = SetUp.PlainStorageAccessorWithOneEntity<QuestionnaireBrowseItem>(
                    id: sourceQuestionnaireId.ToString(), entity: Create.Entity.QuestionnaireBrowseItem(questionnaireIdentity: sourceQuestionnaireId));


            var questionnaire = Create.AggregateRoot.Questionnaire(
                questionnaireBrowseItemStorage: questionnaireBrowseItemStorage,
                questionnaireStorage: plainQuestionnaireRepositoryMock.Object,
                categoriesStorage: categoriesStorage);

            questionnaire.CloneQuestionnaire(Create.Command.CloneQuestionnaire(sourceQuestionnaireId, newQuestionnaireVersion: targetQuestionnaireVersion));

            Assert.That(categoriesStorage.GetOptions(
                    new QuestionnaireIdentity(questionnaireId, targetQuestionnaireVersion), categoriesId),
                Is.EquivalentTo(sourceCategories));

            Assert.That(categoriesStorage.GetOptions(sourceQuestionnaireId, categoriesId),
                Is.EquivalentTo(sourceCategories));
        }
    }
}
