using System;
using System.Collections.Generic;
using System.IO;
using AppDomainToolkit;
using Autofac;
using Main.Core.Documents;
using Ncqrs.Eventing;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.Modularity.Autofac;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Enumerator.Native.Questionnaire;
using WB.Infrastructure.Native.Logging;
using WB.Infrastructure.Native.Monitoring;
using WB.Infrastructure.Native.Storage;
using WB.UI.WebTester.Infrastructure;
using WB.UI.WebTester.Infrastructure.AppDomainSpecific;
using IDisposable = System.IDisposable;

namespace WB.UI.WebTester.Services.Implementation
{
    public class RemoteInterviewContainer : IDisposable
    {
        enum State { Running, Teardown }

        private State state = State.Running;

        public RemoteInterviewContainer(Guid interviewId, string binFolderPath, QuestionnaireDocument questionnaireDocument, List<TranslationDto> translations, string supportingAssembly)
        {
            this.cachePath = Path.Combine(Path.GetTempPath(), "WebTester", interviewId.FormatGuid());
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
            AppDomainsAliveGauge.Inc();

            this.context = domainContext;

            string documentString = JsonConvert.SerializeObject(questionnaireDocument, Formatting.None,
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Objects,
                    NullValueHandling = NullValueHandling.Ignore,
                    FloatParseHandling = FloatParseHandling.Decimal,
                    Formatting = Formatting.None,
                });

            var translationsString = JsonConvert.SerializeObject(translations ?? new List<TranslationDto>(), Formatting.None);

            RemoteAction.Invoke(domainContext.Domain, documentString, translationsString, supportingAssembly,
                (questionnaireJson, translationsJson, assembly) =>
                {
                    try
                    {
                        const int questionnaireVersion = 1;
                        SetupAppDomainsSeviceLocator();

                        QuestionnaireDocument document = JsonConvert.DeserializeObject<QuestionnaireDocument>(
                            questionnaireJson, new JsonSerializerSettings
                            {
                                TypeNameHandling = TypeNameHandling.Objects,
                                NullValueHandling = NullValueHandling.Ignore,
                                FloatParseHandling = FloatParseHandling.Decimal,
                                Formatting = Formatting.None
                            });

                        List<TranslationInstance> translationsList = JsonConvert.DeserializeObject<List<TranslationInstance>>(translationsJson);

                        ServiceLocator.Current.GetInstance<IQuestionnaireAssemblyAccessor>().StoreAssembly(document.PublicKey, questionnaireVersion, assembly);
                        ServiceLocator.Current.GetInstance<IQuestionnaireStorage>().StoreQuestionnaire(document.PublicKey, questionnaireVersion, document);
                        ServiceLocator.Current.GetInstance<IWebTesterTranslationService>().Store(translationsList);
                    }
                    catch
                    {

                    }
                });
        }

        private static void SetupAppDomainsSeviceLocator()
        {
            AutofacKernel kernel = new AutofacKernel();
            kernel.Load(
                new WebTesterAppDomainModule(),
                new NLogLoggingModule()
                );

            kernel.Init().Wait();
        }

        private static readonly Gauge AppDomainsAliveGauge =
            new Gauge(@"wb_app_domains_total", @"Count of appdomains per interview in memory");

        private readonly AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> context;
        private readonly string cachePath;

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

            if (Directory.Exists(cachePath))
            {
                try
                {
                    Directory.Delete(cachePath, true);
                }
                catch (UnauthorizedAccessException exception)
                {
                    //this.logger.Error(
                    //    $"Failed to delete folder during interview tear down. Path: {interviewContainer.CachePath}",
                    //    exception);
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

                var commandString = JsonConvert.SerializeObject(command, Formatting.None, CommandsSerializerSettings);

                string eventsFromCommand = RemoteFunc.Invoke(context.Domain, commandString, cmd =>
                {
                    ICommand deserializedCommand =
                        (ICommand)JsonConvert.DeserializeObject(cmd, CommandsSerializerSettings);
                    var aggregateid = CommandRegistry.GetAggregateRootIdResolver(deserializedCommand)
                        .Invoke(deserializedCommand);

                    if(CurrentInterview.Instance == null)
                        CurrentInterview.Instance = ServiceLocator.Current.GetInstance<StatefulInterview>();
                    StatefulInterview interview = CurrentInterview.Instance;

                    interview.SetId(aggregateid);

                    Action<ICommand, IAggregateRoot> commandHandler =
                        CommandRegistry.GetCommandHandler(deserializedCommand);
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

                List<CommittedEvent> resultResult =
                    JsonConvert.DeserializeObject<List<CommittedEvent>>(eventsFromCommand, EventsSerializerSettings);
                return resultResult;
            }
        }

        public List<CategoricalOption> GetFirstTopFilteredOptionsForQuestion(Identity questionIdentity, int? parentQuestionValue, string filter, int itemsCount = 200)
        {
            lock (this.context)
            {
                if (state == State.Teardown) throw new InterviewException("Interview deleted", InterviewDomainExceptionType.InterviewHardDeleted);

                var sArg = JsonConvert.SerializeObject((questionIdentity, parentQuestionValue, filter, itemsCount),
                       CommandsSerializerSettings);

                var invokeResult = RemoteFunc.Invoke(context.Domain, sArg, jsonArg =>
                {
                    try
                    {
                        var args = JsonConvert
                            .DeserializeObject<(Identity questionIdentity, int? parentQuestionValue, string filter, int
                                    itemsCount)>
                                (jsonArg, CommandsSerializerSettings);

                        var interview = CurrentInterview.Instance;

                        var result = interview.GetFirstTopFilteredOptionsForQuestion(
                            args.questionIdentity,
                            args.parentQuestionValue,
                            args.filter, args.itemsCount);

                        return JsonConvert.SerializeObject(result, CommandsSerializerSettings);
                    }
                    catch
                    {
                        return "[]";
                    }
                });

                return JsonConvert.DeserializeObject<List<CategoricalOption>>(invokeResult, CommandsSerializerSettings);
            }
        }

        private static JsonSerializerSettings EventsSerializerSettings =>
            EventSerializerSettings.BackwardCompatibleJsonSerializerSettings;

        private static JsonSerializerSettings CommandsSerializerSettings => new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects,
            ContractResolver = new PrivateSetterContractResolver(),
            Converters = new JsonConverter[]
                {new StringEnumConverter(), new IdentityJsonConverter(), new RosterVectorConverter()}
        };

        public void Flush()
        {
            lock (this.context)
            {
                RemoteAction.Invoke(context.Domain, () => CurrentInterview.Instance = null);
            }

            
        }
    }
}
