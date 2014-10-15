﻿using System;
using Main.Core.Documents;
using Main.Core.Events.Questionnaire;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Spec;
using NUnit.Framework;
using Newtonsoft.Json;
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
        [TestCase("{ \"Children\": [ { \"$type\": \"Main.Core.Entities.SubEntities.Group, Main.Core\", \"Children\": [ { \"$type\": \"Main.Core.Entities.SubEntities.Question.DateTimeQuestion, Main.Core\", \"AddDateTimeAttr\": null, \"DateTimeAttr\": \"0001-01-01T00:00:00.0000000\", \"AnswerOrder\": \"AsIs\", \"Answers\": [], \"Capital\": false, \"Cards\": [], \"Children\": [], \"Comments\": null, \"ConditionExpression\": \"\", \"ConditionalDependentGroups\": [], \"ConditionalDependentQuestions\": [], \"Featured\": false, \"Instructions\": \"\", \"Mandatory\": false, \"PublicKey\": \"356fd964-e5bf-4082-aee5-53a2b383ee69\", \"QuestionScope\": \"Interviewer\", \"QuestionText\": \"Today's date\", \"QuestionType\": \"DateTime\", \"StataExportCaption\": \"date\", \"ValidationExpression\": \"\", \"ValidationMessage\": \"\", \"LinkedToQuestionId\": null }, { \"$type\": \"Main.Core.Entities.SubEntities.Question.TextQuestion, Main.Core\", \"AddTextAttr\": null, \"AnswerOrder\": \"AsIs\", \"Answers\": [], \"Capital\": false, \"Cards\": [], \"Children\": [], \"Comments\": null, \"ConditionExpression\": \"\", \"ConditionalDependentGroups\": [], \"ConditionalDependentQuestions\": [], \"Featured\": false, \"Instructions\": \"\", \"Mandatory\": false, \"PublicKey\": \"7f88f9da-51e5-4d2d-be11-69b0864b4d3d\", \"QuestionScope\": \"Interviewer\", \"QuestionText\": \"What was the weather on %date% ?\", \"QuestionType\": \"Text\", \"StataExportCaption\": \"weather\", \"ValidationExpression\": \"\", \"ValidationMessage\": \"\", \"LinkedToQuestionId\": null } ], \"ConditionExpression\": \"\", \"Enabled\": true, \"Description\": \"\", \"Propagated\": \"None\", \"PublicKey\": \"5921878e-be22-471d-9cf3-7831e9a9b4a6\", \"Title\": \"Main\", \"Triggers\": [] }, { \"$type\": \"Main.Core.Entities.SubEntities.Group, Main.Core\", \"Children\": [ { \"$type\": \"Main.Core.Entities.SubEntities.Question.AutoPropagateQuestion, Main.Core\", \"AddNumericAttr\": null, \"IntAttr\": 0, \"Triggers\": [ \"497549ff-38f4-4bed-bb61-8ae5358483bc\", \"7a04f498-166f-4278-9628-d809b350627e\" ], \"MaxValue\": 10, \"AnswerOrder\": \"AsIs\", \"Answers\": [], \"Capital\": false, \"Cards\": [], \"Children\": [], \"Comments\": null, \"ConditionExpression\": \"\", \"ConditionalDependentGroups\": [], \"ConditionalDependentQuestions\": [], \"Featured\": false, \"Instructions\": \"\", \"Mandatory\": false, \"PublicKey\": \"7117d8e3-1d97-464f-a900-898bc9f97fa9\", \"QuestionScope\": \"Interviewer\", \"QuestionText\": \"How many people for date of %date%\", \"QuestionType\": \"AutoPropagate\", \"StataExportCaption\": \"people_count\", \"ValidationExpression\": \"\", \"ValidationMessage\": \"\", \"LinkedToQuestionId\": null }, { \"$type\": \"Main.Core.Entities.SubEntities.Group, Main.Core\", \"Children\": [ { \"$type\": \"Main.Core.Entities.SubEntities.Question.TextQuestion, Main.Core\", \"AddTextAttr\": null, \"AnswerOrder\": \"AsIs\", \"Answers\": [], \"Capital\": false, \"Cards\": [], \"Children\": [], \"Comments\": null, \"ConditionExpression\": \"\", \"ConditionalDependentGroups\": [], \"ConditionalDependentQuestions\": [], \"Featured\": false, \"Instructions\": \"\", \"Mandatory\": false, \"PublicKey\": \"539d6830-87e5-4453-88cb-fd53ee0c6151\", \"QuestionScope\": \"Interviewer\", \"QuestionText\": \"What is your name?\", \"QuestionType\": \"Text\", \"StataExportCaption\": \"name\", \"ValidationExpression\": \"\", \"ValidationMessage\": \"\", \"LinkedToQuestionId\": null }, { \"$type\": \"Main.Core.Entities.SubEntities.Question.NumericQuestion, Main.Core\", \"IsInteger\": true, \"AnswerOrder\": \"AsIs\", \"Answers\": [], \"Capital\": false, \"Cards\": [], \"Children\": [], \"Comments\": null, \"ConditionExpression\": \"\", \"ConditionalDependentGroups\": [], \"ConditionalDependentQuestions\": [], \"Featured\": false, \"Instructions\": \"\", \"Mandatory\": false, \"PublicKey\": \"84c8aac6-7442-4155-b6cb-0950ddfcaa4e\", \"QuestionScope\": \"Interviewer\", \"QuestionText\": \"What is age of %name%\", \"QuestionType\": \"Numeric\", \"StataExportCaption\": \"age\", \"ValidationExpression\": \"\", \"ValidationMessage\": \"\", \"LinkedToQuestionId\": null } ], \"ConditionExpression\": \"\", \"Enabled\": true, \"Description\": \"\", \"Propagated\": \"AutoPropagated\", \"PublicKey\": \"497549ff-38f4-4bed-bb61-8ae5358483bc\", \"Title\": \"people\", \"Triggers\": [] }, { \"$type\": \"Main.Core.Entities.SubEntities.Group, Main.Core\", \"Children\": [ { \"$type\": \"Main.Core.Entities.SubEntities.Question.SingleQuestion, Main.Core\", \"AddSingleAttr\": null, \"AnswerOrder\": \"AsIs\", \"Answers\": [ { \"$type\": \"Main.Core.Entities.SubEntities.Answer, Main.Core\", \"AnswerImage\": null, \"AnswerText\": \"Awesome\", \"AnswerType\": \"Select\", \"AnswerValue\": \"1\", \"Mandatory\": false, \"NameCollection\": null, \"PublicKey\": \"574320a6-79b3-449e-bef1-d48702cd2143\" }, { \"$type\": \"Main.Core.Entities.SubEntities.Answer, Main.Core\", \"AnswerImage\": null, \"AnswerText\": \"OK\", \"AnswerType\": \"Select\", \"AnswerValue\": \"2\", \"Mandatory\": false, \"NameCollection\": null, \"PublicKey\": \"e46479c0-93ff-4a3f-89ec-049deb2aa1f7\" }, { \"$type\": \"Main.Core.Entities.SubEntities.Answer, Main.Core\", \"AnswerImage\": null, \"AnswerText\": \"So-so\", \"AnswerType\": \"Select\", \"AnswerValue\": \"3\", \"Mandatory\": false, \"NameCollection\": null, \"PublicKey\": \"268c009a-b5db-476f-844e-dab8bc953daf\" }, { \"$type\": \"Main.Core.Entities.SubEntities.Answer, Main.Core\", \"AnswerImage\": null, \"AnswerText\": \"Bad\", \"AnswerType\": \"Select\", \"AnswerValue\": \"4\", \"Mandatory\": false, \"NameCollection\": null, \"PublicKey\": \"ebbdbec8-9efa-431b-83ac-c13a58949c27\" } ], \"Capital\": false, \"Cards\": [], \"Children\": [], \"Comments\": null, \"ConditionExpression\": \"\", \"ConditionalDependentGroups\": [], \"ConditionalDependentQuestions\": [], \"Featured\": false, \"Instructions\": \"\", \"Mandatory\": false, \"PublicKey\": \"adb871d4-417a-469c-913d-d93c0a5dac7d\", \"QuestionScope\": \"Interviewer\", \"QuestionText\": \"at the age of %age% %name% think about him self\", \"QuestionType\": \"SingleOption\", \"StataExportCaption\": \"mind\", \"ValidationExpression\": \"\", \"ValidationMessage\": \"\", \"LinkedToQuestionId\": null } ], \"ConditionExpression\": \"\", \"Enabled\": true, \"Description\": \"\", \"Propagated\": \"AutoPropagated\", \"PublicKey\": \"7a04f498-166f-4278-9628-d809b350627e\", \"Title\": \"2nd roster\", \"Triggers\": [] } ], \"ConditionExpression\": \"\", \"Enabled\": true, \"Description\": \"\", \"Propagated\": \"None\", \"PublicKey\": \"e85ebddd-b6ea-4505-99ff-763fc97755b7\", \"Title\": \"With Roster\", \"Triggers\": [] } ], \"CloseDate\": null, \"ConditionExpression\": \"\", \"CreationDate\": \"2013-10-16T18:30:34.7118893Z\", \"LastEntryDate\": \"2013-10-16T18:38:44.9133838Z\", \"OpenDate\": null, \"IsDeleted\": false, \"CreatedBy\": null, \"IsPublic\": false, \"Propagated\": \"None\", \"PublicKey\": \"743fede4-ce2f-4618-967e-679ef905ffbd\", \"Title\": \"question with substitution\", \"Description\": null, \"Triggers\": [], \"SharedPersons\": [], \"LastEventSequence\": 13 }")]
        public void ImportQuestionnaire_When_Valid_Questionnaire_Imported_Then_Correct_Event_is_Published(string questionnaireTemplate)
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                Questionnaire questionnaire = CreateQuestionnaire();
                var newState = JsonConvert.DeserializeObject<QuestionnaireDocument>(questionnaireTemplate, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects });

                // act
                questionnaire.ImportFromDesigner(Guid.NewGuid(), newState, false, null);

                // assert
                Assert.That(GetLastEvent<TemplateImported>(eventContext).Source, Is.EqualTo(newState));
            }
        }

        [Test]
        [TestCase("{ \"Children\": [ { \"$type\": \"Main.Core.Entities.SubEntities.Group, Main.Core\", \"Children\": [ { \"$type\": \"Main.Core.Entities.SubEntities.Question.DateTimeQuestion, Main.Core\", \"AddDateTimeAttr\": null, \"DateTimeAttr\": \"0001-01-01T00:00:00.0000000\", \"AnswerOrder\": \"AsIs\", \"Answers\": [], \"Capital\": false, \"Cards\": [], \"Children\": [], \"Comments\": null, \"ConditionExpression\": \"\", \"ConditionalDependentGroups\": [], \"ConditionalDependentQuestions\": [], \"Featured\": false, \"Instructions\": \"\", \"Mandatory\": false, \"PublicKey\": \"356fd964-e5bf-4082-aee5-53a2b383ee69\", \"QuestionScope\": \"Interviewer\", \"QuestionText\": \"Today's date\", \"QuestionType\": \"DateTime\", \"StataExportCaption\": \"date\", \"ValidationExpression\": \"\", \"ValidationMessage\": \"\", \"LinkedToQuestionId\": null }, { \"$type\": \"Main.Core.Entities.SubEntities.Question.TextQuestion, Main.Core\", \"AddTextAttr\": null, \"AnswerOrder\": \"AsIs\", \"Answers\": [], \"Capital\": false, \"Cards\": [], \"Children\": [], \"Comments\": null, \"ConditionExpression\": \"\", \"ConditionalDependentGroups\": [], \"ConditionalDependentQuestions\": [], \"Featured\": false, \"Instructions\": \"\", \"Mandatory\": false, \"PublicKey\": \"7f88f9da-51e5-4d2d-be11-69b0864b4d3d\", \"QuestionScope\": \"Interviewer\", \"QuestionText\": \"What was the weather on %date% ?\", \"QuestionType\": \"Text\", \"StataExportCaption\": \"weather\", \"ValidationExpression\": \"\", \"ValidationMessage\": \"\", \"LinkedToQuestionId\": null } ], \"ConditionExpression\": \"\", \"Enabled\": true, \"Description\": \"\", \"Propagated\": \"None\", \"PublicKey\": \"5921878e-be22-471d-9cf3-7831e9a9b4a6\", \"Title\": \"Main\", \"Triggers\": [] }, { \"$type\": \"Main.Core.Entities.SubEntities.Group, Main.Core\", \"Children\": [ { \"$type\": \"Main.Core.Entities.SubEntities.Question.AutoPropagateQuestion, Main.Core\", \"AddNumericAttr\": null, \"IntAttr\": 0, \"Triggers\": [ \"497549ff-38f4-4bed-bb61-8ae5358483bc\", \"7a04f498-166f-4278-9628-d809b350627e\" ], \"MaxValue\": 10, \"AnswerOrder\": \"AsIs\", \"Answers\": [], \"Capital\": false, \"Cards\": [], \"Children\": [], \"Comments\": null, \"ConditionExpression\": \"\", \"ConditionalDependentGroups\": [], \"ConditionalDependentQuestions\": [], \"Featured\": false, \"Instructions\": \"\", \"Mandatory\": false, \"PublicKey\": \"7117d8e3-1d97-464f-a900-898bc9f97fa9\", \"QuestionScope\": \"Interviewer\", \"QuestionText\": \"How many people for date of %date%\", \"QuestionType\": \"AutoPropagate\", \"StataExportCaption\": \"people_count\", \"ValidationExpression\": \"\", \"ValidationMessage\": \"\", \"LinkedToQuestionId\": null }, { \"$type\": \"Main.Core.Entities.SubEntities.Group, Main.Core\", \"Children\": [ { \"$type\": \"Main.Core.Entities.SubEntities.Question.TextQuestion, Main.Core\", \"AddTextAttr\": null, \"AnswerOrder\": \"AsIs\", \"Answers\": [], \"Capital\": false, \"Cards\": [], \"Children\": [], \"Comments\": null, \"ConditionExpression\": \"\", \"ConditionalDependentGroups\": [], \"ConditionalDependentQuestions\": [], \"Featured\": false, \"Instructions\": \"\", \"Mandatory\": false, \"PublicKey\": \"539d6830-87e5-4453-88cb-fd53ee0c6151\", \"QuestionScope\": \"Interviewer\", \"QuestionText\": \"What is your name?\", \"QuestionType\": \"Text\", \"StataExportCaption\": \"name\", \"ValidationExpression\": \"\", \"ValidationMessage\": \"\", \"LinkedToQuestionId\": null }, { \"$type\": \"Main.Core.Entities.SubEntities.Question.NumericQuestion, Main.Core\", \"IsInteger\": true, \"AnswerOrder\": \"AsIs\", \"Answers\": [], \"Capital\": false, \"Cards\": [], \"Children\": [], \"Comments\": null, \"ConditionExpression\": \"\", \"ConditionalDependentGroups\": [], \"ConditionalDependentQuestions\": [], \"Featured\": false, \"Instructions\": \"\", \"Mandatory\": false, \"PublicKey\": \"84c8aac6-7442-4155-b6cb-0950ddfcaa4e\", \"QuestionScope\": \"Interviewer\", \"QuestionText\": \"What is age of %name%\", \"QuestionType\": \"Numeric\", \"StataExportCaption\": \"age\", \"ValidationExpression\": \"\", \"ValidationMessage\": \"\", \"LinkedToQuestionId\": null } ], \"ConditionExpression\": \"\", \"Enabled\": true, \"Description\": \"\", \"Propagated\": \"AutoPropagated\", \"PublicKey\": \"497549ff-38f4-4bed-bb61-8ae5358483bc\", \"Title\": \"people\", \"Triggers\": [] }, { \"$type\": \"Main.Core.Entities.SubEntities.Group, Main.Core\", \"Children\": [ { \"$type\": \"Main.Core.Entities.SubEntities.Question.MultyOptionsQuestion, Main.Core\", \"AddSingleAttr\": null, \"AnswerOrder\": \"AsIs\", \"Answers\": [ { \"$type\": \"Main.Core.Entities.SubEntities.Answer, Main.Core\", \"AnswerImage\": null, \"AnswerText\": \"Awesome\", \"AnswerType\": \"Select\", \"AnswerValue\": \"1\", \"Mandatory\": false, \"NameCollection\": null, \"PublicKey\": \"574320a6-79b3-449e-bef1-d48702cd2143\" }, { \"$type\": \"Main.Core.Entities.SubEntities.Answer, Main.Core\", \"AnswerImage\": null, \"AnswerText\": \"OK\", \"AnswerType\": \"Select\", \"AnswerValue\": \"2\", \"Mandatory\": false, \"NameCollection\": null, \"PublicKey\": \"e46479c0-93ff-4a3f-89ec-049deb2aa1f7\" }, { \"$type\": \"Main.Core.Entities.SubEntities.Answer, Main.Core\", \"AnswerImage\": null, \"AnswerText\": \"So-so\", \"AnswerType\": \"Select\", \"AnswerValue\": \"3\", \"Mandatory\": false, \"NameCollection\": null, \"PublicKey\": \"268c009a-b5db-476f-844e-dab8bc953daf\" }, { \"$type\": \"Main.Core.Entities.SubEntities.Answer, Main.Core\", \"AnswerImage\": null, \"AnswerText\": \"Bad\", \"AnswerType\": \"Select\", \"AnswerValue\": \"4\", \"Mandatory\": false, \"NameCollection\": null, \"PublicKey\": \"ebbdbec8-9efa-431b-83ac-c13a58949c27\" } ], \"Capital\": false, \"Cards\": [], \"Children\": [], \"Comments\": null, \"ConditionExpression\": \"\", \"ConditionalDependentGroups\": [], \"ConditionalDependentQuestions\": [], \"Featured\": false, \"Instructions\": \"\", \"Mandatory\": false, \"PublicKey\": \"adb871d4-417a-469c-913d-d93c0a5dac7d\", \"QuestionScope\": \"Interviewer\", \"QuestionText\": \"at the age of %age% %name% think about him self\", \"QuestionType\": \"MultyOption\", \"StataExportCaption\": \"mind\", \"ValidationExpression\": \"\", \"ValidationMessage\": \"\", \"LinkedToQuestionId\": null, \"MaxAllowedAnswers\" : \"3\" } ], \"ConditionExpression\": \"\", \"Enabled\": true, \"Description\": \"\", \"Propagated\": \"AutoPropagated\", \"PublicKey\": \"7a04f498-166f-4278-9628-d809b350627e\", \"Title\": \"2nd roster\", \"Triggers\": [] } ], \"ConditionExpression\": \"\", \"Enabled\": true, \"Description\": \"\", \"Propagated\": \"None\", \"PublicKey\": \"e85ebddd-b6ea-4505-99ff-763fc97755b7\", \"Title\": \"With Roster\", \"Triggers\": [] } ], \"CloseDate\": null, \"ConditionExpression\": \"\", \"CreationDate\": \"2013-10-16T18:30:34.7118893Z\", \"LastEntryDate\": \"2013-10-16T18:38:44.9133838Z\", \"OpenDate\": null, \"IsDeleted\": false, \"CreatedBy\": null, \"IsPublic\": false, \"Propagated\": \"None\", \"PublicKey\": \"743fede4-ce2f-4618-967e-679ef905ffbd\", \"Title\": \"question with substitution\", \"Description\": null, \"Triggers\": [], \"SharedPersons\": [], \"LastEventSequence\": 13 }")]
        public void ImportQuestionnaire_When_MultiQuestions_Have_Valid_MaxAllowedAnswers_Then_TemplateImportedEventIsRised(string questionnaireTemplate)
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                Questionnaire questionnaire = CreateQuestionnaire();
                var newState = JsonConvert.DeserializeObject<QuestionnaireDocument>(questionnaireTemplate, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects });

                // act
                questionnaire.ImportFromDesigner(Guid.NewGuid(), newState, false, null);

                // assert
                Assert.That(GetLastEvent<TemplateImported>(eventContext).Source, Is.EqualTo(newState));
            }
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