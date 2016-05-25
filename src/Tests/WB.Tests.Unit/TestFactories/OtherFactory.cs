using System;
using Moq;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using System.Collections.Generic;
using Ncqrs.Eventing.Storage;
using Ncqrs.Spec;
using NSubstitute;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Implementation.Factories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using IEvent = WB.Core.Infrastructure.EventBus.IEvent;
using ILogger = WB.Core.GenericSubdomains.Portable.Services.ILogger;
using Ncqrs.Domain;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using WB.Core.BoundedContexts.Headquarters.EventHandler.WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Implementation.Aggregates;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;

namespace WB.Tests.Unit.TestFactories
{
    internal class OtherFactory
    {
        public CommittedEvent CommittedEvent(string origin = null, Guid? eventSourceId = null, IEvent payload = null,
            Guid? eventIdentifier = null, int eventSequence = 1, Guid? commitId = null)
        {
            return new CommittedEvent(
                commitId ?? Guid.NewGuid(),
                origin,
                eventIdentifier ?? Guid.Parse("44440000444440000004444400004444"),
                eventSourceId ?? Guid.Parse("55550000555550000005555500005555"),
                eventSequence,
                new DateTime(2014, 10, 22),
                0,
                payload ?? Mock.Of<IEvent>());
        }

        public Questionnaire DataCollectionQuestionnaire(
            IPlainQuestionnaireRepository plainQuestionnaireRepository = null,
            IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItemStorage = null,
            IFileSystemAccessor fileSystemAccessor = null)
            => new Questionnaire(
                plainQuestionnaireRepository ?? Mock.Of<IPlainQuestionnaireRepository>(),
                Mock.Of<IQuestionnaireAssemblyFileAccessor>(),
                new ReferenceInfoForLinkedQuestionsFactory(),
                new QuestionnaireRosterStructureFactory(),
                questionnaireBrowseItemStorage ?? Mock.Of<IPlainStorageAccessor<QuestionnaireBrowseItem>>(),
                Mock.Of<IPlainKeyValueStorage<ReferenceInfoForLinkedQuestions>>(),
                Mock.Of<IPlainKeyValueStorage<QuestionnaireRosterStructure>>(),
                Mock.Of<IPlainKeyValueStorage<QuestionnaireQuestionsInfo>>(),
                fileSystemAccessor ?? Mock.Of<IFileSystemAccessor>());

        public EventContext EventContext()
        {
            return new EventContext();
        }

        public Interview Interview(Guid? interviewId = null, IPlainQuestionnaireRepository questionnaireRepository = null,
            IInterviewExpressionStatePrototypeProvider expressionProcessorStatePrototypeProvider = null)
        {
            var interview = new Interview(
                Mock.Of<ILogger>(),
                questionnaireRepository ?? Mock.Of<IPlainQuestionnaireRepository>(),
                expressionProcessorStatePrototypeProvider ?? Stub.InterviewExpressionStateProvider());

            interview.SetId(interviewId ?? Guid.NewGuid());

            return interview;
        }

        public InterviewBinaryDataDescriptor InterviewBinaryDataDescriptor()
        {
            return new InterviewBinaryDataDescriptor(Guid.NewGuid(), "test.jpeg", () => new byte[0]);
        }


        public NavigationState NavigationState(IStatefulInterviewRepository interviewRepository = null)
        {
            var result = new NavigationState(
                Mock.Of<ICommandService>(),
                interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                Mock.Of<IUserInteractionService>(),
                Mock.Of<IUserInterfaceStateService>());
            return result;
        }


        public IPublishableEvent PublishableEvent(Guid? eventSourceId = null, IEvent payload = null)
        {
            return Mock.Of<IPublishableEvent>(_ => _.Payload == (payload ?? Mock.Of<IEvent>()) && _.EventSourceId == (eventSourceId ?? Guid.NewGuid()));
        }

        public IPlainQuestionnaireRepository QuestionnaireRepositoryStubWithOneQuestionnaire(
            Guid questionnaireId, IQuestionnaire questionnaire = null, long? questionnaireVersion = null)
        {
            questionnaire = questionnaire ?? Mock.Of<IQuestionnaire>();

            return Mock.Of<IPlainQuestionnaireRepository>(repository
                => repository.GetHistoricalQuestionnaire(questionnaireId, questionnaireVersion ?? questionnaire.Version) == questionnaire
                && repository.GetHistoricalQuestionnaire(questionnaireId, questionnaireVersion ?? 1) == questionnaire
                && repository.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>()) == questionnaire);
        }

        public StatefulInterview StatefulInterview(Guid? questionnaireId = null, 
            long? questionnaireVersion = null,
            Guid? userId = null, 
            IPlainQuestionnaireRepository questionnaireRepository = null)
        {
            questionnaireId = questionnaireId ?? Guid.NewGuid();
            var statefulInterview = new StatefulInterview(
                Mock.Of<ILogger>(),
                questionnaireRepository ?? Mock.Of<IPlainQuestionnaireRepository>(),
                Stub<IInterviewExpressionStatePrototypeProvider>.WithNotEmptyValues)
            {
                QuestionnaireIdentity = new QuestionnaireIdentity(questionnaireId.Value, questionnaireVersion ?? 1),
            };

            statefulInterview.Apply(new InterviewCreated(userId ?? Guid.NewGuid(), questionnaireId.Value, questionnaireVersion ?? 1));

            return statefulInterview;
        }

        public StatefulInterview StatefulInterview(Guid? questionnaireId = null, Guid? userId = null,
    IQuestionnaire questionnaire = null)
        {
            questionnaireId = questionnaireId ?? Guid.NewGuid();
            var statefulInterview = new StatefulInterview(
                Mock.Of<ILogger>(),
                Mock.Of<IPlainQuestionnaireRepository>(x => x.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>()) == questionnaire),
                Stub<IInterviewExpressionStatePrototypeProvider>.WithNotEmptyValues)
            {
                QuestionnaireIdentity = new QuestionnaireIdentity(questionnaireId.Value, 1),
            };

            statefulInterview.Apply(new InterviewCreated(userId ?? Guid.NewGuid(), questionnaireId.Value, 1));

            return statefulInterview;
        }

        public UncommittedEvent UncommittedEvent(Guid? eventSourceId = null, 
            IEvent payload = null,
            int sequence = 1,
            int initialVersion = 1)
        {
            return new UncommittedEvent(Guid.NewGuid(), eventSourceId ?? Guid.NewGuid(), sequence, initialVersion, DateTime.Now, payload);
        }

        public ISnapshotStore SnapshotStore(Guid aggregateRootId, Snapshot snapshot = null)
        {
            return Mock.Of<ISnapshotStore>(_ => _.GetSnapshot(aggregateRootId, Moq.It.IsAny<int>()) == snapshot);
        }

        public IEventStore EventStore(Guid eventSourceId, IEnumerable<CommittedEvent> committedEvents)
        {
            return Mock.Of<IEventStore>(_ =>
                _.Read(eventSourceId, Moq.It.IsAny<int>()) == new CommittedEventStream(eventSourceId, committedEvents));
        }

        public IAggregateSnapshotter AggregateSnapshotter(EventSourcedAggregateRoot aggregateRoot = null, bool isARLoadedFromSnapshotSuccessfully = false)
        {
            return Mock.Of<IAggregateSnapshotter>(_ =>
                _.TryLoadFromSnapshot(Moq.It.IsAny<Type>(), Moq.It.IsAny<Snapshot>(),
                    Moq.It.IsAny<CommittedEventStream>(), out aggregateRoot) ==
                isARLoadedFromSnapshotSuccessfully);
        }

        public IStatefulInterviewRepository StatefulInterviewRepositoryWith(IStatefulInterview interview)
        {
            var result = Substitute.For<IStatefulInterviewRepository>();
            result.Get(null).ReturnsForAnyArgs(interview);
            return result;
        }
    }
}