using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Questionnaire.Categories;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Enumerator.Native.Questionnaire.Impl;
using WB.Infrastructure.Native.Questionnaire;
using WB.Infrastructure.Native.Questionnaire.Impl;
using WB.Infrastructure.Native.Storage;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Repositories
{
    [TestFixture]
    public class HqQuestionnaireStorageTests
    {
        [Test]
        public void should_extract_and_store_all_questionnaire_entities()
        {
            var textQuestion = Id.g1;
            var numericQuestion = Id.g2;
            var multimediaQuestion = Id.g3;
            
            QuestionnaireDocument questionnaire = Create.Entity.QuestionnaireDocument(Id.g1, null,
                Create.Entity.Group(Id.gA, children: new IComposite[]
                {
                    Create.Entity.TextQuestion(textQuestion, variable: "textQ", preFilled: false),
                    Create.Entity.NumericIntegerQuestion(numericQuestion,  "numeric", isPrefilled: true),
                    Create.Entity.Group(Id.gB, children: new IComposite[]
                    {
                        Create.Entity.MultimediaQuestion(multimediaQuestion, variable: "multimedia", scope: QuestionScope.Supervisor)
                    })
                }));

            var fixture = Create.Other.AutoFixture();

            int id = 0;
            var inmemory = new TestInMemoryWriter<QuestionnaireCompositeItem, int>(i => i == 0 ? ++id : i);
            fixture.Register<IReadSideRepositoryWriter<QuestionnaireCompositeItem, int>>(() => inmemory);
            fixture.Register<INativeReadSideStorage<QuestionnaireCompositeItem, int>>(() => inmemory);
            var storage = fixture.Create<HqQuestionnaireStorage>();

            // act
            storage.StoreQuestionnaire(questionnaire.PublicKey, 1, questionnaire);

            // assert
            var results = inmemory.Query(q => q).ToList();

            results.Should().Contain(q => q.EntityId == textQuestion
                                          && q.Featured == false
                                          && q.QuestionType == QuestionType.Text
                                          && q.ParentId == Id.gA
                                          && q.QuestionScope == QuestionScope.Interviewer);

            results.Should().Contain(q => q.EntityId == numericQuestion
                                          && q.Featured == true
                                          && q.QuestionType == QuestionType.Numeric
                                          && q.ParentId == Id.gA
                                          && q.QuestionScope == QuestionScope.Interviewer);

            results.Should().Contain(q => q.EntityId == multimediaQuestion
                                          && q.Featured == false
                                          && q.QuestionType == QuestionType.Multimedia
                                          && q.ParentId == Id.gB
                                          && q.QuestionScope == QuestionScope.Supervisor);

            results.Should().OnlyContain(q =>
                q.QuestionnaireIdentity == new QuestionnaireIdentity(questionnaire.PublicKey, 1).ToString());
        }       
        
        [Test]
        public void when_load_questionnaire_should_include_reusable_categories_into_questionnaire_document()
        {
            var singleQuestion1 = Id.g1;
            var multiQuestion = Id.g2;
            var singleQuestion2 = Id.g3;

            var category1 = Id.g4;
            var categoryOptions1 = new CategoriesItem[]
            {
                Create.Entity.CategoriesItem("1", 1), 
                Create.Entity.CategoriesItem("2", 2),
            };
            var category2 = Id.g5;
            var categoryOptions2 = new CategoriesItem[]
            {
                Create.Entity.CategoriesItem("11", 11),
                Create.Entity.CategoriesItem("22", 22),
                Create.Entity.CategoriesItem("33", 33),
            };

            QuestionnaireDocument questionnaire = Create.Entity.QuestionnaireDocument(Id.g1, null,
                Create.Entity.Group(Id.gA, children: new IComposite[]
                {
                    Create.Entity.SingleOptionQuestion(singleQuestion1, categoryId: category1, answers: new List<Answer>()),
                    Create.Entity.MultyOptionsQuestion(multiQuestion,  categoryId: category2, options: new List<Answer>()),
                    Create.Entity.Group(Id.gB, children: new IComposite[]
                    {
                        Create.Entity.SingleOptionQuestion(singleQuestion2, categoryId: category2, answers: new List<Answer>())
                    })
                }));
            questionnaire.Categories = new List<Categories>()
            {
                new Categories() { Id = category1, Name = "1"},
                new Categories() { Id = category2, Name = "2"},
            };

            var questionnaireIdentity = Create.Entity.QuestionnaireIdentity(questionnaire.PublicKey, 1);
            var fixture = Create.Other.AutoFixture();

            var reusableCategoriesStorage = Mock.Of<IReusableCategoriesStorage>(s =>
                s.GetOptions(questionnaireIdentity, category1) == categoryOptions1 &&
                s.GetOptions(questionnaireIdentity, category2) == categoryOptions2);

            int id = 0;
            var inmemory = new TestInMemoryWriter<QuestionnaireCompositeItem, int>(i => i == 0 ? ++id : i);
            fixture.Register<IReadSideRepositoryWriter<QuestionnaireCompositeItem, int>>(() => inmemory);
            fixture.Register<INativeReadSideStorage<QuestionnaireCompositeItem, int>>(() => inmemory);
            fixture.Register<IReusableCategoriesStorage>(() => reusableCategoriesStorage);
            fixture.Register<IQuestionOptionsRepository>(() => new QuestionnaireQuestionOptionsRepository());
            fixture.Register<IReusableCategoriesFillerIntoQuestionnaire>(() => new ReusableCategoriesFillerIntoQuestionnaire(reusableCategoriesStorage));
            fixture.Register<IMemoryCache>(() => new MemoryCache(Options.Create(new MemoryCacheOptions())));
            var storage = fixture.Create<HqQuestionnaireStorage>();
            storage.StoreQuestionnaire(questionnaire.PublicKey, 1, questionnaire);

            // act
            var result = storage.GetQuestionnaire(questionnaireIdentity, null);

            // assert
            var optionsForSingleQuestion1 = result.GetOptionsForQuestion(singleQuestion1, null, null, null).ToList();
            CollectionAssert.AreEqual(optionsForSingleQuestion1.Select(o => o.Value), categoryOptions1.Select(o => o.Id));
            CollectionAssert.AreEqual(optionsForSingleQuestion1.Select(o => o.Title), categoryOptions1.Select(o => o.Text));
            CollectionAssert.AreEqual(optionsForSingleQuestion1.Select(o => o.ParentValue), categoryOptions1.Select(o => o.ParentId));

            var optionsForMultiQuestion = result.GetOptionsForQuestion(multiQuestion, null, null, null).ToList();
            CollectionAssert.AreEqual(optionsForMultiQuestion.Select(o => o.Value), categoryOptions2.Select(o => o.Id));
            CollectionAssert.AreEqual(optionsForMultiQuestion.Select(o => o.Title), categoryOptions2.Select(o => o.Text));
            CollectionAssert.AreEqual(optionsForMultiQuestion.Select(o => o.ParentValue), categoryOptions2.Select(o => o.ParentId));

            var optionsForSingleQuestion2 = result.GetOptionsForQuestion(singleQuestion2, null, null, null).ToList();
            CollectionAssert.AreEqual(optionsForSingleQuestion2.Select(o => o.Value), categoryOptions2.Select(o => o.Id));
            CollectionAssert.AreEqual(optionsForSingleQuestion2.Select(o => o.Title), categoryOptions2.Select(o => o.Text));
            CollectionAssert.AreEqual(optionsForSingleQuestion2.Select(o => o.ParentValue), categoryOptions2.Select(o => o.ParentId));
        }
    }
}
