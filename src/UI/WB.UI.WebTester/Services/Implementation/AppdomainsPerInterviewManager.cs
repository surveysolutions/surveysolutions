using System;
using System.Collections.Generic;
using System.IO;
using AppDomainToolkit;
using Autofac;
using Main.Core.Documents;
using Ncqrs.Eventing;
using Newtonsoft.Json;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.Modularity.Autofac;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Infrastructure.Native.Logging;
using WB.Infrastructure.Native.Storage;
using WB.UI.Shared.Enumerator.Services.Internals;
using WB.UI.WebTester.Infrastructure.AppDomainSpecific;

namespace WB.UI.WebTester.Services.Implementation
{
    public class AppdomainsPerInterviewManager : IAppdomainsPerInterviewManager
    {
        private readonly string binFolderPath;
        private const int QuestionnaireVersion = 1;

        class InterviewContainer
        {
            public AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> Context { get; set; }
            public string AssemblyFilePath { get; set; }
        }

        private readonly Dictionary<Guid, InterviewContainer> appDomains =
            new Dictionary<Guid, InterviewContainer>();

        public AppdomainsPerInterviewManager(string binFolderPath)
        {
            this.binFolderPath = binFolderPath;
        }

        public void SetupForInterview(Guid interviewId, QuestionnaireDocument questionnaireDocument,
            string supportingAssembly)
        {
            if (appDomains.ContainsKey(interviewId))
            {
                this.TearDown(interviewId);
            }

            var tempFileName = Path.GetTempFileName();
            File.WriteAllBytes(tempFileName, Convert.FromBase64String(supportingAssembly));

            var setupInfo = new AppDomainSetup
            {
                ApplicationName = interviewId.FormatGuid(),
                ApplicationBase = binFolderPath,
                PrivateBinPath = binFolderPath
            };

            var domainContext = AppDomainContext.Create(setupInfo);
            appDomains[interviewId] = new InterviewContainer
            {
                Context = domainContext,
                AssemblyFilePath = tempFileName
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
                if (File.Exists(interviewContainer.AssemblyFilePath))
                {
                    File.Delete(interviewContainer.AssemblyFilePath);
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
                StatefulInterview interview = ServiceLocator.Current.GetInstance<StatefulInterview>();

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

        private static JsonSerializerSettings EventsSerializerSettings =>
            EventSerializerSettings.BackwardCompatibleJsonSerializerSettings;

        private static JsonSerializerSettings CommandsSerializerSettings => new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects,
            ContractResolver = new PrivateSetterContractResolver()
        };
    }
}