using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Main.Core.Documents;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.BoundedContexts.Supervisor.Services.Implementation;
using WB.Core.BoundedContexts.Supervisor.Views;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.HttpServices.HttpClient;
using WB.Core.Infrastructure.Modularity;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.Enumerator.Implementation.Repositories;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.UI.Shared.Enumerator.Services;
using WB.UI.Shared.Enumerator.Services.Internals;
using IPrincipal = WB.Core.SharedKernels.Enumerator.Services.Infrastructure.IPrincipal;

namespace WB.UI.Supervisor.ServiceLocation
{
    [ExcludeFromCodeCoverage]
    public class SupervisorInfrastructureModule : IModule
    {
        public void Load(IIocRegistry registry)
        {
            registry.BindToRegisteredInterface<IPrincipal, ISupervisorPrincipal>();
            registry.BindAsSingleton<ISupervisorPrincipal, SupervisorPrincipal>();
            registry.Bind<IStringCompressor, JsonCompressor>();

            var pathToLocalDirectory = AndroidPathUtils.GetPathToInternalDirectory();

            registry.BindAsSingletonWithConstructorArguments<IBackupRestoreService, BackupRestoreService>(
                new ConstructorArgument("privateStorage", context => pathToLocalDirectory),
                new ConstructorArgument("encryptionService",
                    context => new RsaEncryptionService(context.Get<ISecureStorage>())),
                new ConstructorArgument("sendTabletInfoRelativeUrl", context => "api/supervisor/v1/tabletInfo"));

            registry.BindWithConstructorArgument<IQuestionnaireAssemblyAccessor, InterviewerQuestionnaireAssemblyAccessor>(
                "assembliesDirectory", "assemblies");
            registry.Bind<ISerializer, PortableJsonSerializer>();
            registry.Bind<IInterviewAnswerSerializer, NewtonInterviewAnswerJsonSerializer>();
            registry.Bind<IJsonAllTypesSerializer, PortableJsonAllTypesSerializer>();

            registry.Bind<IPlainKeyValueStorage<QuestionnaireDocument>, QuestionnaireKeyValueStorage>();

            registry.Bind<IInterviewerInterviewAccessor, InterviewerInterviewAccessor>();
            registry.Bind<IInterviewEventStreamOptimizer, InterviewEventStreamOptimizer>();
            registry.Bind<IQuestionnaireTranslator, QuestionnaireTranslator>();
            registry.Bind<IQuestionnaireStorage, QuestionnaireStorage>();
            registry.Bind<IInterviewerQuestionnaireAccessor, SupervisorQuestionnaireAccessor>();
            registry.Bind<IAssignmentDocumentsStorage, AssignmentDocumentsStorage>();
            registry.Bind<ITabletInfoService, TabletInfoService>();
            registry.BindAsSingleton<IDeviceSynchronizationProgress, DeviceSynchronizationProgress>();
            registry.Bind<ICalendarEventStorage, CalendarEventStorage>();
            registry.Bind<ICalendarEventRemoval, CalendarEventRemoval>();
            
            registry.Bind<IEnumeratorEventStorage, SqliteMultiFilesEventStorage>();
            registry.BindToRegisteredInterface<IEventStore, IEnumeratorEventStorage>();

            registry.BindToConstant(() => new SqliteSettings
            {
                PathToRootDirectory = AndroidPathUtils.GetPathToInternalDirectory(), 
                PathToDatabaseDirectory = AndroidPathUtils.GetPathToSubfolderInLocalDirectory("data"),
                DataDirectoryName = "data",
                InterviewsDirectoryName = "interviews",
            });

            registry.Bind(typeof(IPlainStorage<,>), typeof(SqlitePlainStorageWithWorkspace<,>));
            registry.Bind(typeof(IPlainStorage<>), typeof(SqlitePlainStorageWithWorkspace<>));

            registry.BindAsSingleton<IPlainStorage<SupervisorIdentity>, SqlitePlainStorage<SupervisorIdentity>>();
            registry.BindAsSingleton<IPlainStorage<WorkspaceView>, SqlitePlainStorage<WorkspaceView>>();
        }
    }
}
