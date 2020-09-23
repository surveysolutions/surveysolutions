using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using Ncqrs;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Storage;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Abc;
using WB.Tests.Abc.TestFactories;
using WB.Tests.Integration.CommandServiceTests;
using IEvent = WB.Core.Infrastructure.EventBus.IEvent;

namespace WB.Tests.Integration.InterviewTests.Substitution
{
    [TestFixture]
    public class SubstitutionTests : InterviewTestsContext
    {
        private AppDomainContext appDomainContext;

        [SetUp]
        public void SetupTest()
        {
            appDomainContext = AppDomainContext.Create();
        }

        [TearDown]
        public void TearDown()
        {
            appDomainContext.Dispose();
        }


        [Test]
        public void when_substitution_changed_should_fire_event_and_dont_save_it_in_event_store()
        {
            var results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.TextQuestion(Id.g1, variable: "text"),
                    Create.Entity.NumericIntegerQuestion(Id.g2, variable: "num", questionText: "%text% correct?")
                );

                var interview = SetupStatefullInterview(appDomainContext.AssemblyLoadContext, questionnaireDocument);

                CommandRegistry
                    .Setup<StatefulInterview>()
                    .InitializesWith<CreateInterview>(command => command.InterviewId, (command, aggregate) => aggregate.CreateInterview(command))
                    .Handles<AnswerTextQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerTextQuestion(command.UserId, command.QuestionId, command.RosterVector, command.OriginDate, command.Answer));

                var repository = Mock.Of<IEventSourcedAggregateRootRepository>(_
                    => _.GetLatest(typeof(StatefulInterview), interview.Id) == interview);

                using var eventContext = new EventContext();
                var eventStore = Create.Storage.InMemoryEventStore();
                var eventBus = Create.Service.LiteEventBus(eventStore: eventStore);

                var commandService = Create.Service.CommandService(repository: repository, eventBus: eventBus);

                // Act
                commandService.Execute(Create.Command.AnswerTextQuestionCommand(interview.Id, Guid.NewGuid(), Id.g1, "answer text"), null);

                var substitutionTitlesChangedEvent = eventContext.GetSingleEvent<SubstitutionTitlesChanged>();
                var eventsInStore = eventStore.Read(interview.Id, 0).ToList();

                return new
                {
                    HasSubstitutionTitlesChangedEvent = substitutionTitlesChangedEvent != null,
                };
            });

            Assert.That(results.HasSubstitutionTitlesChangedEvent, Is.True);
        }

        [Test]
        public void when_load_interview_without_substitution_events_should_set_correct_text_in_all_places()
        {
            var userId = Guid.NewGuid();
            var interviewId = Guid.NewGuid();
            int sequenceCounter = 1;
            var questionnaireIdentity = Create.Entity.QuestionnaireIdentity(Id.gA, 1);

            var results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.TextQuestion(Id.g1, variable: "text"),
                    Create.Entity.NumericIntegerQuestion(Id.g2, variable: "num", questionText: "%text% correct?")
                );
                questionnaireDocument.IsUsingExpressionStorage = true;
                var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument, questionnaireIdentity.Version,
                    questionOptionsRepository: Mock.Of<IQuestionOptionsRepository>());
                questionnaire.ExpressionStorageType = GetInterviewExpressionStorage(appDomainContext.AssemblyLoadContext, questionnaireDocument).GetType();

                var questionnaireRepository = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(
                                                  questionnaireIdentity.QuestionnaireId,
                                                  questionnaire,
                                                  questionnaireIdentity.Version);
                SetUp.InstanceToMockedServiceLocator(questionnaireRepository);

                var interviewLocator = new StatefulInterview(
                    Create.Service.SubstitutionTextFactory(),
                    Create.Service.InterviewTreeBuilder(),
                    Mock.Of<IQuestionOptionsRepository>());

                interviewLocator.ServiceLocatorInstance = ServiceLocator.Current;
                SetUp.InstanceToMockedServiceLocator<StatefulInterview>(interviewLocator);

                ServiceFactory factory = new ServiceFactory();
                var eventStore = Create.Storage.InMemoryEventStore();
                var events = new IEvent[]
                {
                    Create.Event.InterviewCreated(),
                    Create.Event.SupervisorAssigned(userId, Id.gA),
                    Create.Event.InterviewerAssigned(userId, Id.gB),
                    Create.Event.TextQuestionAnswered(userId: userId, questionId: Id.g1, answer: "test answer"),
                };
                eventStore.Store(new UncommittedEventStream(null, events.Select(e =>
                    new UncommittedEvent(Guid.NewGuid(),
                        interviewId,
                        sequenceCounter++,
                        0,
                        DateTime.UtcNow,
                        e))));
                var repository = new DomainRepository(ServiceLocator.Current);
                var aggregateRootRepository = factory.EventSourcedAggregateRootRepository(eventStore, repository);
                var interview = (StatefulInterview)aggregateRootRepository.GetLatest(typeof(StatefulInterview), interviewId);

                var titleText = interview.GetTitleText(Create.Identity(Id.g2));

                return new
                {
                    TitleText = titleText,
                };
            });

            Assert.That(results.TitleText, Is.EqualTo("test answer correct?"));
        }
    }
}
