using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.Repositories
{
    class QuestionniareStorageTests
    {
        [Test]
        public void When_questionnaire_document_replaced_Should_replace_PlainQuestionnaire()
        {
            var questionniareId = Id.gA;
            var questionnaireIdentity = new QuestionnaireIdentity(questionniareId, 1);
            var document1 = Create.Entity.QuestionnaireDocumentWithOneQuestion(questionId: Id.g1, questionnaireId: questionniareId);
            var document2 = Create.Entity.QuestionnaireDocumentWithOneQuestion(questionId: Id.g2, questionnaireId: questionniareId);

            var repository = Create.Storage.QuestionnaireStorage();

            // act
            repository.StoreQuestionnaire(questionniareId, questionnaireIdentity.Version, document1);
            var plainQuestionnaire1 = repository.GetQuestionnaire(questionnaireIdentity, null);
            repository.StoreQuestionnaire(questionniareId, questionnaireIdentity.Version, document2);
            var plainQuestionnaire2 = repository.GetQuestionnaire(questionnaireIdentity, null);

            // Assert
            Assert.That(plainQuestionnaire1.HasQuestion(Id.g1), Is.True);
            Assert.That(plainQuestionnaire1.HasQuestion(Id.g2), Is.False);

            Assert.That(plainQuestionnaire2.HasQuestion(Id.g1), Is.False);
            Assert.That(plainQuestionnaire2.HasQuestion(Id.g2), Is.True);
            
        }
    }
}
