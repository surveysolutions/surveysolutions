using System;
using System.Collections.Generic;
using System.IO;
using Autofac;
using Ncqrs.Eventing;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Implementation.Providers;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Infrastructure.Native.Monitoring;
using WB.UI.WebTester.Infrastructure;

namespace WB.UI.WebTester.Services.Implementation
{
    public class RemoteInterviewContainer : IDisposable
    {
        enum State { Running, Teardown }

        private State state = State.Running;

        public RemoteInterviewContainer(ILifetimeScope rootScope,
            Guid interviewId,
            QuestionnaireIdentity identity,
            string supportingAssembly)
        {
            this.scope = rootScope.BeginLifetimeScope();

            using var assemblyStream = new MemoryStream(Convert.FromBase64String(supportingAssembly));
            context = new InterviewAssemblyLoadContext();
            var assembly = context.LoadFromStream(assemblyStream);

            AppDomainsAliveGauge.Inc();

            var questionnaireStorage = this.scope.Resolve<IQuestionnaireStorage>();
            var questionnaireAssemblyAccessor =  this.scope.Resolve<IQuestionnaireAssemblyAccessor>();

            ((WebTesterQuestionnaireAssemblyAccessor)questionnaireAssemblyAccessor).Assembly = assembly;

            var questionnaire =
                questionnaireStorage.GetQuestionnaire(
                    identity, null);
            var prototype =  new InterviewExpressionStatePrototypeProvider(questionnaireAssemblyAccessor,
                new InterviewExpressionStateUpgrader(),
                this.scope.Resolve<ILoggerProvider>());

            if (questionnaire == null)
                throw new InvalidOperationException("Questionnaire must not be null.");

            questionnaire.ExpressionStorageType = prototype.GetExpressionStorageType(identity);

            statefulInterview = this.scope.Resolve<WebTesterStatefulInterview>();
            statefulInterview.SetId(interviewId);
        }

        private static readonly Gauge AppDomainsAliveGauge =
            new Gauge(@"wb_app_domains_total", @"Count of appdomains per interview in memory");

        private readonly InterviewAssemblyLoadContext context;
        private readonly ILifetimeScope scope;

        public WebTesterStatefulInterview? statefulInterview { get; private set; }

        private void ReleaseUnmanagedResources()
        {
            if (this.context != null)
            {
                lock (this.context)
                {
                    state = State.Teardown;
                    this.context.Dispose();
                }
            }
            this.scope.Dispose();
            AppDomainsAliveGauge.Dec();
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        ~RemoteInterviewContainer()
        {
            ReleaseUnmanagedResources();
        }

        public List<CommittedEvent> Execute(ICommand command)
        {
            lock (this.context)
            {
                if (state == State.Teardown) throw new InterviewException("Interview deleted", InterviewDomainExceptionType.InterviewHardDeleted);
                {
                    Action<ICommand, IAggregateRoot> commandHandler =
                        CommandRegistry.GetCommandHandler(command);

                    if(statefulInterview == null)
                        throw new InvalidOperationException("StatefulInterview must not be null.");

                    commandHandler.Invoke(command, statefulInterview);

                    var eventStream = new UncommittedEventStream(null, statefulInterview.GetUnCommittedChanges());
                    statefulInterview.MarkChangesAsCommitted();

                    List<CommittedEvent> committedEvents = new List<CommittedEvent>();
                    foreach (var uncommittedEvent in eventStream)
                    {
                        var committedEvent = new CommittedEvent(eventStream.CommitId,
                            uncommittedEvent.Origin,
                            uncommittedEvent.EventIdentifier,
                            uncommittedEvent.EventSourceId,
                            uncommittedEvent.EventSequence,
                            uncommittedEvent.EventTimeStamp,
                            0,
                            uncommittedEvent.Payload);
                        committedEvents.Add(committedEvent);
                    }

                    return committedEvents;
                }
            }
        }

        public int? GetLastEventSequence()
        {
            lock (this.context)
            {
                if (state == State.Teardown) throw new InterviewException("Interview deleted", InterviewDomainExceptionType.InterviewHardDeleted);

                if (statefulInterview == null)
                    throw new InvalidOperationException("StatefulInterview must not be null.");

                return statefulInterview.Version;
            }
        }

        public void Flush()
        {
            this.statefulInterview = null;
        }
    }
}
