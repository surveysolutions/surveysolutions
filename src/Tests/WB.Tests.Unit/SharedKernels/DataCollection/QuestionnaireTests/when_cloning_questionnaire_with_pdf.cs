using System;
using System.Linq;
using FluentAssertions;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Implementation.Aggregates;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Enumerator.Native.Questionnaire;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;


namespace WB.Tests.Unit.SharedKernels.DataCollection.QuestionnaireTests
{
    internal class when_cloning_questionnaire_with_pdf : QuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            
            sourceQuestionnaireId = Create.Entity.QuestionnaireIdentity(questionnaireId, 4);

            pdfs = new TestInMemoryKeyValueStorage<QuestionnairePdf>();
            pdfs.Store(Create.Entity.QuestionnairePdf(), sourceQuestionnaireId.ToString());
            

            var questionnaireDocument = Create.Entity.QuestionnaireDocument();
            questionnaireDocument.PublicKey = sourceQuestionnaireId.QuestionnaireId;

            var plainQuestionnaireRepositoryMock = 
                Mock.Get(Mock.Of<IQuestionnaireStorage>(_ => 
                    _.GetQuestionnaireDocument(sourceQuestionnaireId.QuestionnaireId, sourceQuestionnaireId.Version) == questionnaireDocument));

            IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItemStorage
              = SetUp.PlainStorageAccessorWithOneEntity<QuestionnaireBrowseItem>(
                  id: sourceQuestionnaireId.ToString(), entity: Create.Entity.QuestionnaireBrowseItem(questionnaireIdentity: sourceQuestionnaireId));


            questionnaire = Create.AggregateRoot.Questionnaire(
                pdfStorage: pdfs,
                questionnaireBrowseItemStorage: questionnaireBrowseItemStorage,
                questionnaireStorage: plainQuestionnaireRepositoryMock.Object);

            BecauseOf();
        }

        public void BecauseOf() => questionnaire.CloneQuestionnaire(Create.Command.CloneQuestionnaire(sourceQuestionnaireId, newQuestionnaireVersion: targetQuestionnaireVersion));

        [NUnit.Framework.Test] public void should_store_copy_of_pdf () => 
            pdfs.GetById(Create.Entity.QuestionnaireIdentity(questionnaireId, targetQuestionnaireVersion).ToString()).Should().NotBeNull();

        static Guid questionnaireId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        static Questionnaire questionnaire;
        static QuestionnaireIdentity sourceQuestionnaireId;
        static IPlainKeyValueStorage<QuestionnairePdf> pdfs;
        static int targetQuestionnaireVersion = 5;
    }
}
