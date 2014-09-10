﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.BoundedContexts.Supervisor.Questionnaires;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Implementation;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Supervisor.Tests.Synchronization.QuestionnaireSynchronizerTests
{
    class when_pull_delete_questionnaire_entity_and_interview_created_by_this_template_is_present_but_cant_be_deleted : QuestionnaireSynchronizerTestContext
    {
        Establish context = () =>
        {
            deleteLocalQuestionnaireFeedEntry = CreateLocalQuestionnaireFeedEntry(Guid.NewGuid(), QuestionnaireEntryType.QuestionnaireDeleted,
                Guid.NewGuid(), 1);

            IEnumerable<LocalQuestionnaireFeedEntry> localQuestionnaireFeedEntres = new[]
            {
                deleteLocalQuestionnaireFeedEntry
            };


            plainStorageMock = new Mock<IQueryablePlainStorageAccessor<LocalQuestionnaireFeedEntry>>();
            plainStorageMock.Setup(
                x => x.Query(Moq.It.IsAny<Func<IQueryable<LocalQuestionnaireFeedEntry>, IQueryable<LocalQuestionnaireFeedEntry>>>()))
                .Returns(localQuestionnaireFeedEntres.AsQueryable());

            headquartersQuestionnaireReaderMock = new Mock<IHeadquartersQuestionnaireReader>();
            headquartersQuestionnaireReaderMock.Setup(x => x.GetQuestionnaireByUri(Moq.It.IsAny<Uri>()))
                .Returns(Task.FromResult(new QuestionnaireDocument()));


            plainQuestionnaireRepositoryMock = new Mock<IPlainQuestionnaireRepository>();

            headquartersPullContext = new HeadquartersPullContextTestable();

            var interviewsMock = new Mock<IQueryableReadSideRepositoryWriter<InterviewSummary>>();
            interviewsMock.Setup(x => x.QueryAll(Moq.It.IsAny<Expression<Func<InterviewSummary, bool>>>())).Returns(new[]
            {
                new InterviewSummary() { WasCreatedOnClient = true, InterviewId = censusModeInterviewId }
            });

            commandServiceMock = new Mock<ICommandService>();
            commandServiceMock.Setup(x => x.Execute(
                Moq.It.Is<HardDeleteInterview>(
                    c =>
                        c.InterviewId == censusModeInterviewId),
                Moq.It.IsAny<string>())).Throws<ArgumentException>();

            questionnaireSynchronizer = CreateQuestionnaireSynchronizer(plainStorage: plainStorageMock.Object,
                plainQuestionnaireRepository: plainQuestionnaireRepositoryMock.Object,
                headquartersQuestionnaireReader: headquartersQuestionnaireReaderMock.Object, headquartersPullContext: headquartersPullContext, interviews: interviewsMock.Object, commandService: commandServiceMock.Object);
        };

        Because of = () =>
            questionnaireSynchronizer.Pull();

        It should_1_questionnaire_be_not_deleted_in_plain_questionnaire_repository = () =>
            plainQuestionnaireRepositoryMock.Verify(x => x.DeleteQuestionnaireDocument(deleteLocalQuestionnaireFeedEntry.QuestionnaireId, deleteLocalQuestionnaireFeedEntry.QuestionnaireVersion), Times.Never);

        It should_DeleteQuestionnaire_command_never_be_called = () =>
            commandServiceMock.Verify(
                x =>
                    x.Execute(
                        Moq.It.Is<DeleteQuestionnaire>(
                            c =>
                                c.QuestionnaireId == deleteLocalQuestionnaireFeedEntry.QuestionnaireId &&
                                    c.QuestionnaireVersion == deleteLocalQuestionnaireFeedEntry.QuestionnaireVersion),
                        Moq.It.IsAny<string>()), Times.Never);


        private static QuestionnaireSynchronizer questionnaireSynchronizer;
        private static LocalQuestionnaireFeedEntry deleteLocalQuestionnaireFeedEntry;
        private static Mock<IQueryablePlainStorageAccessor<LocalQuestionnaireFeedEntry>> plainStorageMock;
        private static Mock<IHeadquartersQuestionnaireReader> headquartersQuestionnaireReaderMock;
        private static Mock<IPlainQuestionnaireRepository> plainQuestionnaireRepositoryMock;
        private static Mock<ICommandService> commandServiceMock;
        private static HeadquartersPullContextTestable headquartersPullContext;

        private static Guid censusModeInterviewId = Guid.NewGuid();
    }
}
