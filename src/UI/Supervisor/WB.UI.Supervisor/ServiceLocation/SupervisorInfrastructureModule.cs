using System.IO;
using System.Threading.Tasks;
using Main.Core.Documents;
using Ncqrs.Eventing.Storage;
using NLog;
using NLog.Config;
using NLog.Targets;
using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.BoundedContexts.Supervisor.Services.Implementation;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Modularity;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Implementation.Repositories;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.UI.Shared.Enumerator.Services;
using WB.UI.Shared.Enumerator.Services.Internals;
using WB.UI.Shared.Enumerator.Services.Logging;
using IPrincipal = WB.Core.SharedKernels.Enumerator.Services.Infrastructure.IPrincipal;

namespace WB.UI.Supervisor.ServiceLocation
{
    public class SupervisorInfrastructureModule : IModule
    {
        public void Load(IIocRegistry registry)
        {
            registry.BindToRegisteredInterface<IPrincipal, ISupervisorPrincipal>();
            registry.BindAsSingleton<ISupervisorPrincipal, SupervisorPrincipal>();
            registry.Bind<IStringCompressor, JsonCompressor>();

            var pathToLocalDirectory = AndroidPathUtils.GetPathToInternalDirectory();

            var fileName = Path.Combine(pathToLocalDirectory, "Logs", "${shortdate}.log");
            var fileTarget = new FileTarget("logFile")
            {
                FileName = fileName,
                Layout = "${longdate}[${logger}][${level}][${message}][${onexception:${exception:format=toString,Data}|${stacktrace}}]"
            };

            var config = new LoggingConfiguration();
            config.AddTarget("logFile", fileTarget);
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Warn, fileTarget));
            LogManager.Configuration = config;

            registry.BindAsSingletonWithConstructorArgument<IBackupRestoreService, BackupRestoreService>(
                "privateStorage", pathToLocalDirectory);

            registry.Bind<ILoggerProvider, NLogLoggerProvider>();

            registry.BindAsSingletonWithConstructorArgument<IQuestionnaireAssemblyAccessor, InterviewerQuestionnaireAssemblyAccessor>(
                "pathToAssembliesDirectory", AndroidPathUtils.GetPathToSubfolderInLocalDirectory("assemblies"));
            registry.Bind<ISerializer, PortableJsonSerializer>();
            registry.Bind<IInterviewAnswerSerializer, PortableInterviewAnswerJsonSerializer>();
            registry.Bind<IJsonAllTypesSerializer, PortableJsonAllTypesSerializer>();

            registry.BindAsSingleton<IPlainKeyValueStorage<QuestionnaireDocument>, QuestionnaireKeyValueStorage>();

            registry.Bind<IInterviewerInterviewAccessor, InterviewerInterviewAccessor>();
            registry.Bind<IInterviewEventStreamOptimizer, InterviewEventStreamOptimizer>();
            registry.Bind<IQuestionnaireTranslator, QuestionnaireTranslator>();
            registry.BindAsSingleton<IQuestionnaireStorage, QuestionnaireStorage>();
            registry.Bind<IAudioFileStorage, InterviewerAudioFileStorage>();
            registry.Bind<IImageFileStorage, InterviewerImageFileStorage>();
            registry.Bind<IAnswerToStringConverter, AnswerToStringConverter>();
            registry.Bind<IInterviewerQuestionnaireAccessor, SupervisorQuestionnaireAccessor>();
            registry.BindAsSingleton<IAssignmentDocumentsStorage, AssignmentDocumentsStorage>();
            registry.BindAsSingleton<IAuditLogService, EnumeratorAuditLogService>();
            registry.BindAsSingleton<ITabletInfoService, TabletInfoService>();
            
            registry.BindAsSingleton<IEnumeratorEventStorage, SqliteMultiFilesEventStorage>();
            registry.BindToRegisteredInterface<IEventStore, IEnumeratorEventStorage>();

            registry.BindToConstant(() => new SqliteSettings
            {
                PathToDatabaseDirectory = AndroidPathUtils.GetPathToSubfolderInLocalDirectory("data"),
                PathToInterviewsDirectory = AndroidPathUtils.GetPathToSubfolderInLocalDirectory($"data{Path.DirectorySeparatorChar}interviews")
            });

            registry.BindAsSingleton(typeof(IPlainStorage<,>), typeof(SqlitePlainStorage<,>));
            registry.BindAsSingleton(typeof(IPlainStorage<>), typeof(SqlitePlainStorage<>));
        }

        public Task Init(IServiceLocator serviceLocator) => Task.CompletedTask;
    }
}
