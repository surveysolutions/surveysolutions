using System.Linq;
using AutoFixture;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
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
            
            QuestionnaireDocument questionnaire = Create.Entity.QuestionnaireDocument(Id.g1,
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

            var inmemory = new TestPlainStorage<QuestionnaireCompositeItem>();
            fixture.Register<IPlainStorageAccessor<QuestionnaireCompositeItem>>(() => inmemory);
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
    }
}
