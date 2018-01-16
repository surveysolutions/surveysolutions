using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Subjects;
using AppDomainToolkit;
using Autofac;
using Main.Core.Documents;
using Ncqrs.Eventing;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.Modularity.Autofac;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Infrastructure.Native.Logging;
using WB.Infrastructure.Native.Storage;
using WB.UI.Shared.Enumerator.Services.Internals;
using WB.UI.WebTester.Infrastructure.AppDomainSpecific;
using IDisposable = System.IDisposable;

namespace WB.UI.WebTester.Services.Implementation
{
    public class AppdomainsPerInterviewManager : IAppdomainsPerInterviewManager, IDisposable
    {
        private readonly string binFolderPath;
        private readonly IDisposable evictNotification;
        private readonly ILogger logger;
        private const int QuestionnaireVersion = 1;

        class InterviewContainer
        {
            public AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> Context { get; set; }
            public string CachePath { get; set; }
        }

        private readonly Dictionary<Guid, InterviewContainer> appDomains =
            new Dictionary<Guid, InterviewContainer>();

        public AppdomainsPerInterviewManager(string binFolderPath,
            IObservable<Guid> evictNotification,
            ILogger logger)
        {
            this.binFolderPath = binFolderPath ?? throw new ArgumentNullException(nameof(binFolderPath));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            this.evictNotification = evictNotification?.Subscribe(TearDown);
        }

        public void SetupForInterview(Guid interviewId, QuestionnaireDocument questionnaireDocument, string supportingAssembly)
        {
            if (appDomains.ContainsKey(interviewId))
            {
                this.TearDown(interviewId);
            }

            var cachePath = Path.Combine(Path.GetTempPath(), interviewId.FormatGuid());
            Directory.CreateDirectory(cachePath);
            var rulesAssemblyPath = Path.Combine(cachePath, "rules.dll");
            File.WriteAllBytes(rulesAssemblyPath, Convert.FromBase64String(supportingAssembly));

            var setupInfo = new AppDomainSetup
            {
                ApplicationName = interviewId.FormatGuid(),
                ApplicationBase = binFolderPath,
                CachePath = cachePath,
                ShadowCopyFiles = "true"
            };

            var domainContext = AppDomainContext.Create(setupInfo);
            appDomains[interviewId] = new InterviewContainer
            {
                Context = domainContext,
                CachePath = cachePath
            };

            string documentString = JsonConvert.SerializeObject(questionnaireDocument, Formatting.None,
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Objects,
                    NullValueHandling = NullValueHandling.Ignore,
                    FloatParseHandling = FloatParseHandling.Decimal,
                    Formatting = Formatting.None,
                });

            RemoteAction.Invoke(domainContext.Domain,
                documentString, supportingAssembly,
                (questionnaire, assembly) =>
                {
                    SetupAppDomainsSeviceLocator();

                    QuestionnaireDocument document1 = JsonConvert.DeserializeObject<QuestionnaireDocument>(
                        questionnaire, new JsonSerializerSettings
                        {
                            TypeNameHandling = TypeNameHandling.Objects,
                            NullValueHandling = NullValueHandling.Ignore,
                            FloatParseHandling = FloatParseHandling.Decimal,
                            Formatting = Formatting.None,
                        });
                    ServiceLocator.Current.GetInstance<IQuestionnaireAssemblyAccessor>().StoreAssembly(document1.PublicKey, QuestionnaireVersion, assembly);
                    ServiceLocator.Current.GetInstance<IQuestionnaireStorage>().StoreQuestionnaire(document1.PublicKey, QuestionnaireVersion, document1);
                });
        }

        private static void SetupAppDomainsSeviceLocator()
        {
            ContainerBuilder containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterModule(new WebTesterAppDomainModule().AsAutofac());
            containerBuilder.RegisterModule(new NLogLoggingModule().AsAutofac());

            var container = containerBuilder.Build();
            ServiceLocator.SetLocatorProvider(() => new AutofacServiceLocatorAdapter(container));
        }

        public void TearDown(Guid interviewId)
        {
            if (this.appDomains.ContainsKey(interviewId))
            {
                var interviewContainer = this.appDomains[interviewId];
                interviewContainer.Context.Dispose();
                if (Directory.Exists(interviewContainer.CachePath))
                {
                    try
                    {
                        Directory.Delete(interviewContainer.CachePath, true);
                    }
                    catch (UnauthorizedAccessException exception)
                    {
                        this.logger.Error($"Failed to delete folder during interview tear down. Path: {interviewContainer.CachePath}", exception);
                    }
                }

                this.appDomains.Remove(interviewId);
            }
        }

        public List<CommittedEvent> Execute(ICommand command)
        {
            var interviewCommand = command as InterviewCommand;
            var appDomain = appDomains[interviewCommand.InterviewId];

            var commandString = JsonConvert.SerializeObject(command, Formatting.None, CommandsSerializerSettings);
            
            string eventsFromCommand = RemoteFunc.Invoke(appDomain.Context.Domain, commandString, cmd =>
            {
                ICommand deserializedCommand = (ICommand)JsonConvert.DeserializeObject(cmd, CommandsSerializerSettings);
                var aggregateid = CommandRegistry.GetAggregateRootIdResolver(deserializedCommand).Invoke(deserializedCommand);

                StatefulInterview interview = ServiceLocator.Current.GetInstance<StatefulInterview>();

                interview.SetId(aggregateid);

                Action<ICommand, IAggregateRoot> commandHandler = CommandRegistry.GetCommandHandler(deserializedCommand);
                commandHandler.Invoke(deserializedCommand, interview);

                var eventStream = new UncommittedEventStream(null, interview.GetUnCommittedChanges());
                interview.MarkChangesAsCommitted();

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

                List<CommittedEvent> eve = committedEvents;
                var result = JsonConvert.SerializeObject(eve, EventsSerializerSettings);
                return result;
            });

            List<CommittedEvent> resultResult = JsonConvert.DeserializeObject<List<CommittedEvent>>(eventsFromCommand, EventsSerializerSettings);
            return resultResult;
        }

        public List<CategoricalOption> FilteredCategoricalOptions(Guid interviewId, CategoricalOptionsFilter categoricalOptionsFilter)
        {
            var appDomain = appDomains[interviewId];

            var sArg = JsonConvert.SerializeObject(categoricalOptionsFilter, CommandsSerializerSettings);

            var invokeResult = RemoteFunc.Invoke(appDomain.Context.Domain, sArg, jsonArg =>
            {
                var filter = JsonConvert.DeserializeObject<CategoricalOptionsFilter>(jsonArg, CommandsSerializerSettings);

                var interview = ServiceLocator.Current.GetInstance<StatefulInterview>();

                var result = interview.FilteredCategoricalOptions(
                    filter.QuestionIdentity,
                    filter.ItemsCount,
                    filter.UnfilteredOptionsForQuestion);

                return JsonConvert.SerializeObject(result, CommandsSerializerSettings);
            });

            return JsonConvert.DeserializeObject<List<CategoricalOption>>(invokeResult, CommandsSerializerSettings);
        }

        private static JsonSerializerSettings EventsSerializerSettings =>
            EventSerializerSettings.BackwardCompatibleJsonSerializerSettings;

        private static JsonSerializerSettings CommandsSerializerSettings => new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects,
            ContractResolver = new PrivateSetterContractResolver(),
            Converters = new JsonConverter[] { new StringEnumConverter(), new IdentityJsonConverter(), new RosterVectorConverter() }
        };

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.evictNotification.Dispose();

                }
                
                disposedValue = true;
            }
        }
        
        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}