using System;
using Moq;
using Ncqrs.Spec;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using WB.Core.BoundedContexts.Headquarters.Commands;
using WB.Core.BoundedContexts.Headquarters.Implementation.Aggregates;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;
using WB.Tests.Abc.TestFactories;

namespace WB.Tests.Unit.SharedKernels.DataCollection.QuestionnaireTests
{
    internal class ImportQuestionnaireTests : QuestionnaireTestsContext
    {
        [SetUp]
        public void SetUp()
        {
            AssemblyContext.SetupServiceLocator();
        }

        [Test]
        public void DeleteQuestionnaire_When_Valid_Questionnaire_Imported_but_not_disabled_for_delete_with_specified_responsible_Then_QuestionnaireException_thrown()
        {
            // arrange
            var responsibleId = Guid.Parse("11111111111111111111111111111111");
            Questionnaire questionnaire = CreateImportedQuestionnaire(creatorId: responsibleId);
            var newState = CreateQuestionnaireDocumentWithOneChapter();

            using (var eventContext = new EventContext())
            {
                questionnaire.ImportFromDesigner(Create.Event.ImportFromDesigner(responsibleId, newState, false, "base64 string of assembly", 1));

                TestDelegate act = () => questionnaire.DeleteQuestionnaire(new DeleteQuestionnaire(Guid.NewGuid(), 1, responsibleId));

                // assert
                Assert.Throws<QuestionnaireException>(act);
            }
        }

        [Test]
        public void DeleteQuestionnaire_When_Valid_Questionnaire_execute_DeleteQuestionnaire_command()
        {
            // arrange
            var responsibleId = Guid.Parse("11111111111111111111111111111111");
            Guid questionnaireId = Guid.NewGuid() ;
            long questionnaireVersion = 7;

            var questionnaireDocument = CreateQuestionnaireDocumentWithOneChapter();
            var plainQuestionnaireRepository = Mock.Of<IQuestionnaireStorage>(r => 
                r.GetQuestionnaireDocument(questionnaireId, questionnaireVersion) == questionnaireDocument);
            var questionnaireBrowseItem = new EntityFactory().QuestionnaireBrowseItem(questionnaireId, questionnaireVersion);
            var questionnaireBrowseItemAccessor = Mock.Of<IPlainStorageAccessor<QuestionnaireBrowseItem>>(a =>
                a.GetById(Moq.It.IsAny<object>()) == questionnaireBrowseItem);

            var questionnaire = Create.AggregateRoot.Questionnaire(
                questionnaireStorage: plainQuestionnaireRepository,
                questionnaireBrowseItemStorage: questionnaireBrowseItemAccessor);

            questionnaire.DisableQuestionnaire(Create.Command.DisableQuestionnaire(questionnaireId, questionnaireVersion, responsibleId));
            questionnaire.DeleteQuestionnaire(Create.Command.DeleteQuestionnaire(questionnaireId, questionnaireVersion, responsibleId));

            // assert
            ClassicAssert.AreEqual(questionnaireDocument.IsDeleted, true);
            ClassicAssert.AreEqual(questionnaireBrowseItem.IsDeleted, true);
        }

        [Test]
        public void DisableQuestionnaire_When_Questionnaire_is_absent_Then_QuestionnaireException_thrown()
        {
            // arrange
            var responsibleId = Guid.Parse("11111111111111111111111111111111");
            Questionnaire questionnaire = CreateImportedQuestionnaire(creatorId: responsibleId);

            var questionnaireBrowseItemStorage =
             Mock.Of<IPlainStorageAccessor<QuestionnaireBrowseItem>>();

            Abc.SetUp.InstanceToMockedServiceLocator(questionnaireBrowseItemStorage);

            using (var eventContext = new EventContext())
            {

                TestDelegate act = () => questionnaire.DisableQuestionnaire(new DisableQuestionnaire(Guid.NewGuid(), 3, responsibleId));

                // assert
                Assert.Throws<QuestionnaireException>(act);
            }
        }

        [Test]
        public void DisableQuestionnaire_When_Valid_Questionnaire_Imported_but_already_disabled_Then_QuestionnaireException_thrown()
        {
            // arrange
            var responsibleId = Guid.Parse("11111111111111111111111111111111");
            Questionnaire questionnaire = CreateImportedQuestionnaire(creatorId: responsibleId);
            var newState = CreateQuestionnaireDocumentWithOneChapter();

            var questionnaireBrowseItemStorage =
                Mock.Of<IPlainStorageAccessor<QuestionnaireBrowseItem>>(_ => _.GetById(Moq.It.IsAny<object>())== new QuestionnaireBrowseItem() { Disabled = true });

            Abc.SetUp.InstanceToMockedServiceLocator(questionnaireBrowseItemStorage);
            using (var eventContext = new EventContext())
            {
                TestDelegate act =
                    () =>
                        questionnaire.DisableQuestionnaire(new DisableQuestionnaire(Guid.NewGuid(), 1, responsibleId));

                // assert
                Assert.Throws<QuestionnaireException>(act);
            }
        }

        [Test]
        public void DeleteQuestionnaire_When_Valid_Questionnaire_Version_is_invalid_Imported_Then_QuestionnaireException_sould_be_thrown()
        {
            // arrange
            Questionnaire questionnaire = CreateImportedQuestionnaire();

            // act

            Assert.Throws<QuestionnaireException>(() => questionnaire.DeleteQuestionnaire(new DeleteQuestionnaire(Guid.NewGuid(), 2, null)));
        }

        [Test]
        public void
            RegisterPlainQuestionnaire_When_Valid_Questionnaire_Is_Deleted_From_Plain_Storage_Imported_Then_Correct_Event_is_Published()
        {
            // arrange
            var document = CreateQuestionnaireDocumentWithOneChapter();
            document.IsDeleted = true;

            var plainQuestionnaireRepository =
                Mock.Of<IQuestionnaireStorage>(_ => _.GetQuestionnaireDocument(document.PublicKey, 3) == document);

            Abc.SetUp.InstanceToMockedServiceLocator<IQuestionnaireStorage>(plainQuestionnaireRepository);


            Questionnaire questionnaire = CreateImportedQuestionnaire();

            // act and assert
            Assert.Throws<QuestionnaireException>(() => questionnaire.RegisterPlainQuestionnaire(new RegisterPlainQuestionnaire(document.PublicKey, 3, false, null)));
        }
    }
}
