using System;
using System.Collections.Generic;
using System.IO;
using Main.Core.Documents;
using Ncqrs.Eventing;
using Newtonsoft.Json;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Implementation.Providers;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Enumerator.Native.Questionnaire;
using WB.Infrastructure.Native.Monitoring;
using WB.UI.WebTester.Infrastructure;

namespace WB.UI.WebTester.Services.Implementation
{
    public class RemoteInterviewContainer : IDisposable
    {
        private readonly ILoggerProvider loggerProvider;

        enum State { Running, Teardown }

        private State state = State.Running;

        public RemoteInterviewContainer(Guid interviewId, 
            string binFolderPath, 
            QuestionnaireDocument questionnaireDocument, 
            List<TranslationDto> translations,
            ILoggerProvider loggerProvider,
            string supportingAssembly)
        {
            this.loggerProvider = loggerProvider;
            using (var assemblyStream = new MemoryStream(Convert.FromBase64String(supportingAssembly)))
            {
                context = new InterviewAssemblyLoadContext(binFolderPath);
                var assembly = context.LoadFromStream(assemblyStream);

                AppDomainsAliveGauge.Inc();

                string documentString = JsonConvert.SerializeObject(questionnaireDocument, Formatting.None,
                    new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.Objects,
                        NullValueHandling = NullValueHandling.Ignore,
                        FloatParseHandling = FloatParseHandling.Decimal,
                        Formatting = Formatting.None,
                    });

                var translationsString =
                    JsonConvert.SerializeObject(translations ?? new List<TranslationDto>(), Formatting.None);

                const int questionnaireVersion = 1;

                QuestionnaireDocument document = JsonConvert.DeserializeObject<QuestionnaireDocument>(
                    documentString, new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.Objects,
                        NullValueHandling = NullValueHandling.Ignore,
                        FloatParseHandling = FloatParseHandling.Decimal,
                        Formatting = Formatting.None
                    });

                List<TranslationInstance> translationsList =
                    JsonConvert.DeserializeObject<List<TranslationInstance>>(translationsString);

                var questionnaireStorage = ServiceLocator.Current.GetInstance<IQuestionnaireStorage>();
                var webTesterTranslationService = ServiceLocator.Current.GetInstance<IWebTesterTranslationService>();
                var questionnaireAssemblyAccessor = ServiceLocator.Current.GetInstance<IQuestionnaireAssemblyAccessor>();

                questionnaireStorage.StoreQuestionnaire(document.PublicKey, questionnaireVersion, document);
                
                webTesterTranslationService.Store(translationsList);
                ((WebTesterQuestionnaireAssemblyAccessor)questionnaireAssemblyAccessor).Assembly = assembly;

                var questionnaireIdentity = new QuestionnaireIdentity(document.PublicKey, questionnaireVersion);
                var questionnaire =
                    questionnaireStorage.GetQuestionnaire(
                        questionnaireIdentity, null);
                var prototype = new InterviewExpressionStatePrototypeProvider(questionnaireAssemblyAccessor,
                    new InterviewExpressionStateUpgrader(),
                    loggerProvider);

                questionnaire.ExpressionStorageType = prototype.GetExpressionStorageType(questionnaireIdentity);

                statefulInterview = ServiceLocator.Current.GetInstance<WebTesterStatefulInterview>();
                statefulInterview.SetId(interviewId);
            }
        }

        private static readonly Gauge AppDomainsAliveGauge =
            new Gauge(@"wb_app_domains_total", @"Count of appdomains per interview in memory");

        private readonly InterviewAssemblyLoadContext context;

        public WebTesterStatefulInterview statefulInterview { get; private set; }

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

                return statefulInterview.Version;
            }
        }

        public void Flush()
        {
            this.statefulInterview = null;
        }
    }
}
