﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NSubstitute;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Commands;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Services.DeleteQuestionnaireTemplate;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Commands.Assignment;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.DeleteQuestionnaireServiceTests
{
    internal class DeleteQuestionnaireServiceTests : DeleteQuestionnaireServiceTestContext
    {
        [Test]
        public void when_delete_questionnaire_and_lookup_tables()
        {
            var questionnaireIdentity = Create.Entity.QuestionnaireIdentity(Id.g1, 5);

            Guid userId = Guid.Parse("22222222222222222222222222222222");
            Guid lookup1 = Id.g1;
            Guid lookup2 = Id.g2;

            var commandServiceMock = Substitute.For<ICommandService>();

            var questionnaire = Create.Entity.QuestionnaireDocument(Id.gA);
            questionnaire.LookupTables = new Dictionary<Guid, LookupTable>
            {
                { lookup1, new LookupTable() },
                { lookup2, new LookupTable() },
            };

            var questionnaireStorage = Mock.Of<IQuestionnaireStorage>(s => s.GetQuestionnaireDocument(questionnaireIdentity) == questionnaire);

            var lookupStorage = new Mock<IPlainKeyValueStorage<QuestionnaireLookupTable>>();

            void SetupLookupStorage(Guid lookupId) => lookupStorage
                .Setup(s => s.GetById(GetLookupKey(questionnaireIdentity, lookupId)))
                .Returns(new QuestionnaireLookupTable());

            SetupLookupStorage(lookup1);
            SetupLookupStorage(lookup2);

            SetUp.InstanceToMockedServiceLocator(questionnaireStorage);
            SetUp.InstanceToMockedServiceLocator(lookupStorage.Object);

            var interviewsToDeleteFactoryMock = new Mock<IInterviewsToDeleteFactory>();

            var deleteQuestionnaireService = CreateDeleteQuestionnaireService(commandService: commandServiceMock,
                interviewsToDeleteFactory: interviewsToDeleteFactoryMock.Object,
                questionnaireStorage: questionnaireStorage,
                lookupStorage : lookupStorage.Object,
                questionnaireBrowseItemStorage:
                    Mock.Of<IPlainStorageAccessor<QuestionnaireBrowseItem>>(
                        _ =>
                            _.GetById(It.IsAny<string>()) ==
                            new QuestionnaireBrowseItem
                            {
                                Disabled = false,
                                QuestionnaireId = questionnaireIdentity.QuestionnaireId,
                                Version = questionnaireIdentity.Version
                            }));

            deleteQuestionnaireService.DeleteInterviewsAndQuestionnaireAfter(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version, userId);
            
            Mock.Get(questionnaireStorage).Verify(s => s.GetQuestionnaireDocument(questionnaireIdentity), Times.Once);
            
            lookupStorage.Verify(l => l.Remove(GetLookupKey(questionnaireIdentity, lookup1)), Times.Once);
            lookupStorage.Verify(l => l.Remove(GetLookupKey(questionnaireIdentity, lookup2)), Times.Once);
        }

        string GetLookupKey(QuestionnaireIdentity questionnaireIdentity, Guid lookupId) => LookupStorageHelpers.GetLookupKey(null, questionnaireIdentity, lookupId);
        
        [Test]
        public async Task when_delete_questionnaire_and_one_interview()
        {
            Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
            long questionnaireVersion = 5;
            Guid userId = Guid.Parse("22222222222222222222222222222222");
            var questionnaireIdentity = new QuestionnaireIdentity(questionnaireId, questionnaireVersion);
            Guid interviewId = Guid.Parse("33333333333333333333333333333333");

            var commandServiceMock = Substitute.For<ICommandService>();
            

            var questionnaire = Create.Entity.QuestionnaireDocument();
            questionnaire.LookupTables = new Dictionary<Guid, LookupTable>();

            var plainQuestionnaireRepository = Mock.Of<IQuestionnaireStorage>(s => s.GetQuestionnaireDocument(It.IsAny<QuestionnaireIdentity>()) == questionnaire);
            SetUp.InstanceToMockedServiceLocator(plainQuestionnaireRepository);

            var interviewsToDeleteFactoryMock = new Mock<IInterviewsToDeleteFactory>();

            var deleteQuestionnaireService = CreateDeleteQuestionnaireService(commandService: commandServiceMock,
                interviewsToDeleteFactory: interviewsToDeleteFactoryMock.Object,
                questionnaireStorage: plainQuestionnaireRepository,
                questionnaireBrowseItemStorage:
                    Mock.Of<IPlainStorageAccessor<QuestionnaireBrowseItem>>(
                        _ =>
                            _.GetById(It.IsAny<string>()) ==
                            new QuestionnaireBrowseItem
                            {
                                Disabled = false,
                                QuestionnaireId = questionnaireId,
                                Version = questionnaireVersion
                            }));

            await deleteQuestionnaireService.DisableQuestionnaire(questionnaireId, questionnaireVersion, userId);
            deleteQuestionnaireService.DeleteInterviewsAndQuestionnaireAfter(questionnaireId, questionnaireVersion, userId);

            interviewsToDeleteFactoryMock.Verify(c => c.RemoveAllEventsForInterviews(It.Is<QuestionnaireIdentity>(q => q == questionnaireIdentity)), Times.Once);
            interviewsToDeleteFactoryMock.Verify(c => c.RemoveAudioForInterviews(It.Is<QuestionnaireIdentity>(q => q == questionnaireIdentity)), Times.Once);
            interviewsToDeleteFactoryMock.Verify(c => c.RemoveAudioAuditForInterviews(It.Is<QuestionnaireIdentity>(q => q == questionnaireIdentity)), Times.Once);
            interviewsToDeleteFactoryMock.Verify(c => c.RemoveAllInterviews(It.Is<QuestionnaireIdentity>(q => q == questionnaireIdentity)), Times.Once);
            
            commandServiceMock.Received(1).Execute(
                Arg.Is<DisableQuestionnaire>(
                    _ =>
                        _.QuestionnaireId == questionnaireId && _.QuestionnaireVersion == questionnaireVersion &&
                        _.ResponsibleId == userId), Arg.Any<string>());

            commandServiceMock.Received(1)
                .Execute(Arg.Is<DeleteQuestionnaire>(
                    _ =>
                        _.QuestionnaireId == questionnaireId && _.QuestionnaireVersion == questionnaireVersion &&
                        _.ResponsibleId == userId), Arg.Any<string>());
        }

        [Test]
        public async Task when_delete_questionnaire_and_one_assignment()
        {
            Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
            long questionnaireVersion = 5;
            var questionnaireIdentity = new QuestionnaireIdentity(questionnaireId, questionnaireVersion);
            Guid userId = Guid.Parse("22222222222222222222222222222222");
            Guid assignmentId = Guid.Parse("33333333333333333333333333333333");

            var commandServiceMock = Substitute.For<ICommandService>();


            var questionnaire = Create.Entity.QuestionnaireDocument();
            questionnaire.LookupTables = new Dictionary<Guid, LookupTable>();

            var plainQuestionnaireRepository = Mock.Of<IQuestionnaireStorage>(s => s.GetQuestionnaireDocument(It.IsAny<QuestionnaireIdentity>()) == questionnaire);
            SetUp.InstanceToMockedServiceLocator(plainQuestionnaireRepository);

            var assignmentsToDeleteFactoryMock = new Mock<IAssignmentsToDeleteFactory>();

            var deleteQuestionnaireService = CreateDeleteQuestionnaireService(commandService: commandServiceMock,
                assignmentsToDeleteFactory: assignmentsToDeleteFactoryMock.Object,
                questionnaireStorage: plainQuestionnaireRepository,
                questionnaireBrowseItemStorage:
                    Mock.Of<IPlainStorageAccessor<QuestionnaireBrowseItem>>(
                        _ =>
                            _.GetById(It.IsAny<string>()) ==
                            new QuestionnaireBrowseItem
                            {
                                Disabled = false,
                                QuestionnaireId = questionnaireId,
                                Version = questionnaireVersion
                            }));

            await deleteQuestionnaireService.DisableQuestionnaire(questionnaireId, questionnaireVersion, userId);
            deleteQuestionnaireService.DeleteInterviewsAndQuestionnaireAfter(questionnaireId, questionnaireVersion, userId);

            assignmentsToDeleteFactoryMock.Verify(a => a.RemoveAllAssignments(
                    It.Is<QuestionnaireIdentity>(q => q == questionnaireIdentity)), Times.Once);
            assignmentsToDeleteFactoryMock.Verify(a => a.RemoveAllEventsForAssignments(
                    It.Is<QuestionnaireIdentity>(q => q == questionnaireIdentity)), Times.Once);

            commandServiceMock.Received(1).Execute(
                Arg.Is<DisableQuestionnaire>(
                    _ =>
                        _.QuestionnaireId == questionnaireId && _.QuestionnaireVersion == questionnaireVersion &&
                        _.ResponsibleId == userId), Arg.Any<string>());

            commandServiceMock.Received(1)
                .Execute(Arg.Is<DeleteQuestionnaire>(
                    _ =>
                        _.QuestionnaireId == questionnaireId && _.QuestionnaireVersion == questionnaireVersion &&
                        _.ResponsibleId == userId), Arg.Any<string>());
        }
    }
}
