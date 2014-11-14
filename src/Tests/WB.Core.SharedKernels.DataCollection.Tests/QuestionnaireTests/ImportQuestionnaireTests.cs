using System;
using Main.Core.Documents;
using Main.Core.Events.Questionnaire;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernels.DataCollection.Events.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.SharedKernels.DataCollection.Tests.QuestionnaireTests
{
    internal class ImportQuestionnaireTests : QuestionnaireTestsContext
    {
        [SetUp]
        public void SetUp()
        {
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);
            NcqrsEnvironment.SetGetter<ILogger>(Mock.Of<ILogger>);
            NcqrsEnvironment.SetGetter<IUniqueIdentifierGenerator>(Mock.Of<IUniqueIdentifierGenerator>);
            NcqrsEnvironment.SetGetter<IClock>(Mock.Of<IClock>);;
        }

        [Test]
        public void CreateNewSnapshot_When_ArgumentIsNotNull_Then_TemplateImportedEventIsRised()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                Questionnaire questionnaire = CreateQuestionnaire();
                var newState = CreateQuestionnaireDocumentWithOneChapter();

                // act
                questionnaire.ImportFromDesigner(Guid.NewGuid(),newState, false, null);

                // assert
                Assert.That(GetLastEvent<TemplateImported>(eventContext).Source, Is.EqualTo(newState));
            }
        }


        [Test]
        public void Execute_When_SourceIsNotQuestionnaireDocument_Then_QuestionnaireException_should_be_thrown()
        {
            // arrange
            Questionnaire questionnaire = CreateQuestionnaire();
            Mock<IQuestionnaireDocument> docMock = new Mock<IQuestionnaireDocument>();
            
            // act
            TestDelegate act =
                () =>
                questionnaire.ImportFromDesigner(Guid.NewGuid(), docMock.Object, false, null);
            
            // assert
            Assert.Throws<QuestionnaireException>(act);
            
        }

        [Test]
        public void ImportFromSupervisor_When_ArgumentIsNotNull_Then_Correct_Event_is_Published()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                Questionnaire questionnaire = CreateQuestionnaire();
                var newState = CreateQuestionnaireDocumentWithOneChapter();

                // act
                questionnaire.ImportFromSupervisor(newState);

                // assert
                Assert.That(GetLastEvent<TemplateImported>(eventContext).Source, Is.EqualTo(newState));
            }
        }

        [Test]
        public void ImportFromDesignerForTester_When_ArgumentIsNotNull_Then_Correct_Event_is_Published()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                Questionnaire questionnaire = CreateQuestionnaire();
                var newState = CreateQuestionnaireDocumentWithOneChapter();

                // act
                questionnaire.ImportFromDesignerForTester(newState);

                // assert
                Assert.That(GetLastEvent<TemplateImported>(eventContext).Source, Is.EqualTo(newState));
                Assert.That(GetLastEvent<TemplateImported>(eventContext).Version, Is.EqualTo(2));
            }
        }


        [Test]
        public void DeleteQuestionnaire_When_Valid_Questionnaire_Imported_with_specified_version_Then_QuestionnaireDeleted_Event_is_Published_with_specified_version()
        {
            // arrange
            var responsibleId = Guid.Parse("11111111111111111111111111111111");
            Questionnaire questionnaire = CreateQuestionnaire(creatorId: responsibleId);
            var newState = CreateQuestionnaireDocumentWithOneChapter();

            using (var eventContext = new EventContext())
            {
                questionnaire.ImportFromDesigner(responsibleId, newState, false, null);
                // act
                questionnaire.DeleteQuestionnaire(1, responsibleId);
                // assert
                var lastEvent = GetLastEvent<QuestionnaireDeleted>(eventContext);
            
                Assert.That(lastEvent.QuestionnaireVersion, Is.EqualTo(1));
            }
        }

        [Test]
        public void DeleteQuestionnaire_When_Valid_Questionnaire_Imported_with_specified_responsible_Then_QuestionnaireDeleted_Event_is_Published_with_specified_responsible()
        {
            // arrange
            var responsibleId = Guid.Parse("11111111111111111111111111111111");
            Questionnaire questionnaire = CreateQuestionnaire(creatorId: responsibleId);
            var newState = CreateQuestionnaireDocumentWithOneChapter();

            using (var eventContext = new EventContext())
            {
                questionnaire.ImportFromDesigner(responsibleId, newState, false, null);
                // act
                questionnaire.DeleteQuestionnaire(1, responsibleId);
                // assert
                var lastEvent = GetLastEvent<QuestionnaireDeleted>(eventContext);

                Assert.That(lastEvent.ResponsibleId, Is.EqualTo(responsibleId));
            }
        }

        [Test]
        public void DeleteQuestionnaire_When_Valid_Questionnaire_Version_is_invalid_Imported_Then_QuestionnaireException_sould_be_thrown()
        {
            // arrange
            Questionnaire questionnaire = CreateQuestionnaire();

            // act

            Assert.Throws<QuestionnaireException>(() => questionnaire.DeleteQuestionnaire(2, null));
        }

        [Test]
        public void QuestionnaireCtor_When_ArgumentIsNotNull_Then_Correct_Event_is_Published()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                var newState = CreateQuestionnaireDocumentWithOneChapter();

                // act
                Questionnaire questionnaire = new Questionnaire(newState);
               
                // assert
                Assert.That(GetLastEvent<TemplateImported>(eventContext).Source, Is.EqualTo(newState));
            }
        }


        [Test]
        public void ImportFromDesignerForTester_When_Valid_Questionnaire_Imported_Then_Correct_Event_is_Published()
        {

            using (var eventContext = new EventContext())
            {
                // arrange
                Questionnaire questionnaire = CreateQuestionnaire();
                var document = CreateQuestionnaireDocumentWithOneChapter();
                // act
                questionnaire.ImportFromDesignerForTester(document);

                // assert
                Assert.That(GetLastEvent<TemplateImported>(eventContext).Source, Is.EqualTo(document));
            }
        }

        [Test]
        public void ImportImportFromSupervisor_When_Valid_Questionnaire_Imported_Then_Correct_Event_is_Published()
        {

            using (var eventContext = new EventContext())
            {
                // arrange
                Questionnaire questionnaire = CreateQuestionnaire();
                var document = CreateQuestionnaireDocumentWithOneChapter();

                // act
                questionnaire.ImportFromSupervisor(document);

                // assert
                Assert.That(GetLastEvent<TemplateImported>(eventContext).Source, Is.EqualTo(document));
                Assert.That(GetLastEvent<TemplateImported>(eventContext).Version, Is.EqualTo(2));
            }
        }

        [Test]
        public void RegisterPlainQuestionnaire_When_Valid_Questionnaire_Imported_Then_Correct_Event_is_Published()
        {

            using (var eventContext = new EventContext())
            {
                // arrange
                Questionnaire questionnaire = CreateQuestionnaire();
                var document = CreateQuestionnaireDocumentWithOneChapter();

                // act
                questionnaire.RegisterPlainQuestionnaire(document.PublicKey, 3, false, null);

                // assert
                Assert.That(GetLastEvent<PlainQuestionnaireRegistered>(eventContext).AllowCensusMode, Is.EqualTo(false));
                Assert.That(GetLastEvent<PlainQuestionnaireRegistered>(eventContext).Version, Is.EqualTo(3));
            }
        }


        [Test]
        public void
            RegisterPlainQuestionnaire_When_Valid_Questionnaire_Is_Deleted_From_Plain_Storage_Imported_Then_Correct_Event_is_Published()
        {
            // arrange
            var document = CreateQuestionnaireDocumentWithOneChapter();
            document.IsDeleted = true;

            var plainQuestionnaireRepository =
                Mock.Of<IPlainQuestionnaireRepository>(_ => _.GetQuestionnaireDocument(document.PublicKey, 3) == document);

            var serviceLocatorMock = new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock };
            serviceLocatorMock.Setup(x => x.GetInstance<IPlainQuestionnaireRepository>()).Returns(plainQuestionnaireRepository);
            ServiceLocator.SetLocatorProvider(() => serviceLocatorMock.Object);
            Questionnaire questionnaire = CreateQuestionnaire();

            // act and assert
            Assert.Throws<QuestionnaireException>(() => questionnaire.RegisterPlainQuestionnaire(document.PublicKey, 3, false, null));
        }

        [Test]
        public void ImportFromDesigner_When_Valid_Questionnaire_but_previouse_version_was_deleted_Imported_Then_Correct_Event_is_Published()
        {

            using (var eventContext = new EventContext())
            {
                // arrange
                var responsibleId = Guid.Parse("11111111111111111111111111111111");
                Questionnaire questionnaire = CreateQuestionnaire(creatorId: responsibleId);
                var document = CreateQuestionnaireDocumentWithOneChapter();

                // act
                questionnaire.ImportFromDesigner(responsibleId, document, false, null);
                questionnaire.DeleteQuestionnaire(2, responsibleId);
                questionnaire.ImportFromDesigner(responsibleId, document, false, null);

                // assert
                Assert.That(GetLastEvent<TemplateImported>(eventContext).Version, Is.EqualTo(3));
            }
        }

    }
}