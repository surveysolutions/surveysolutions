    using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using AppDomainToolkit;
using Autofac;
using Main.Core.Documents;
using Ncqrs.Eventing;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
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
using WB.UI.WebTester.Infrastructure;
    using WB.UI.WebTester.Infrastructure.AppDomainSpecific;

namespace WB.UI.WebTester.Services.Implementation
{
    public class AppdomainsPerInterviewManager : IAppdomainsPerInterviewManager
    {
        private readonly string binFolderPath;
        private const int QuestionnaireVersion = 1;

        private readonly Dictionary<Guid, AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver>> appDomains = 
            new Dictionary<Guid, AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver>>();

        public AppdomainsPerInterviewManager(string binFolderPath)
        {
            this.binFolderPath = binFolderPath;
        }

        public void SetupForInterview(Guid interviewId, QuestionnaireDocument questionnaireDocument, 
            string supportingAssembly)
        {
            if (appDomains.ContainsKey(interviewId))
            {
                appDomains[interviewId].Dispose();
                appDomains.Remove(interviewId);
            }

            var tempFileName = Path.GetTempFileName();
            try
            {
                File.WriteAllBytes(tempFileName, Convert.FromBase64String(supportingAssembly));

                var setupInfo = new AppDomainSetup()
                {
                    ApplicationName = interviewId.FormatGuid(),
                    ApplicationBase = binFolderPath,
                    PrivateBinPath = binFolderPath 
                };

                var domainContext = AppDomainContext.Create(setupInfo);
                appDomains[interviewId] = domainContext;
                

                
                //domainContext.AssemblyImporter.AddProbePath(binFolderPath);
                //foreach (var directory in Directory.GetDirectories(binFolderPath))
                //{
                //    foreach (var subDirectory in Directory.GetDirectories(directory))
                //    {
                //        domainContext.AssemblyImporter.AddProbePath(subDirectory);
                //    }
                //}
                domainContext.LoadAssembly(LoadMethod.LoadFile, tempFileName);

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
            finally
            {
                //File.Delete(tempFileName);
            }
        }

        private static void SetupAppDomainsSeviceLocator()
        {
            ContainerBuilder containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterModule(new WebTesterAppDomainModule().AsAutofac());
            containerBuilder.RegisterModule(new NLogLoggingModule().AsAutofac());

            var container = containerBuilder.Build();
            ServiceLocator.SetLocatorProvider(() => new AutofacServiceLocatorAdapter(container));
        }

        public void Dispose(Guid interviewId)
        {
            throw new NotImplementedException();
        }

        public List<CommittedEvent> Execute(ICommand command)
        {
            var interviewCommand = command as InterviewCommand;
            var appDomain = appDomains[interviewCommand.InterviewId];

            var commandString = JsonConvert.SerializeObject(command, Formatting.None, CommandsSerializerSettings);

            string eventsFromCommand = RemoteFunc.Invoke(appDomain.Domain, commandString, cmd =>
            {
                ICommand deserializedCommand = (ICommand) JsonConvert.DeserializeObject(cmd, CommandsSerializerSettings);
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