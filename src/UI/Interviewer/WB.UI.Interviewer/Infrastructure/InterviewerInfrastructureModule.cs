using System.IO;
using System.Threading.Tasks;
using Main.Core.Documents;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Interviewer.Implementation;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views;
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
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.UI.Interviewer.Services;
using IPrincipal = WB.Core.SharedKernels.Enumerator.Services.Infrastructure.IPrincipal;
using WB.UI.Shared.Enumerator.Services;
using WB.UI.Shared.Enumerator.Services.Internals;
using WB.UI.Shared.Enumerator.Services.Notifications;

namespace WB.UI.Interviewer.Infrastructure
{
    public class  InterviewerInfrastructureModule : IModule
    {
        public void Load(IIocRegistry registry)
        {
            registry.BindToRegisteredInterface<IPrincipal, IInterviewerPrincipal>();
            registry.BindAsSingleton<IInterviewerPrincipal, InterviewerPrincipal>();
            registry.Bind<IStringCompressor, JsonCompressor>();

            var pathToLocalDirectory = AndroidPathUtils.GetPathToInternalDirectory();

            registry.BindAsSingletonWithConstructorArguments<IBackupRestoreService, BackupRestoreService>(
                new ConstructorArgument("privateStorage", context => pathToLocalDirectory),
                new ConstructorArgument("encryptionService",
                    context => new RsaEncryptionService(context.Get<ISecureStorage>())),
                new ConstructorArgument("sendTabletInfoRelativeUrl", context => "api/interviewer/v2/tabletInfo"));

            registry.BindAsSingletonWithConstructorArgument<IQuestionnaireAssemblyAccessor, InterviewerQuestionnaireAssemblyAccessor>(
                "pathToAssembliesDirectory", AndroidPathUtils.GetPathToSubfolderInLocalDirectory("assemblies"));
            registry.Bind<ISerializer, PortableJsonSerializer>();
            registry.Bind<IInterviewAnswerSerializer, NewtonInterviewAnswerJsonSerializer>();
            registry.Bind<IJsonAllTypesSerializer, PortableJsonAllTypesSerializer>();

            registry.Bind<IPlainKeyValueStorage<QuestionnaireDocument>, QuestionnaireKeyValueStorage>();

            registry.Bind<IInterviewerQuestionnaireAccessor, InterviewerQuestionnaireAccessor>();
            registry.Bind<IInterviewerInterviewAccessor, InterviewerInterviewAccessor>();
            registry.Bind<IInterviewEventStreamOptimizer, InterviewEventStreamOptimizer>();
            registry.Bind<IQuestionnaireTranslator, QuestionnaireTranslator>();
            registry.Bind<IQuestionnaireStorage, QuestionnaireStorage>();
            registry.Bind<IAssignmentDocumentsStorage, AssignmentDocumentsStorage>();
            registry.Bind<IAudioAuditService, AudioAuditService>();
            
            registry.Bind<IEnumeratorEventStorage, SqliteMultiFilesEventStorage>();
            registry.BindToRegisteredInterface<IEventStore, IEnumeratorEventStorage>();

            registry.BindToConstant(() => new SqliteSettings
            {
                PathToDatabaseDirectory = AndroidPathUtils.GetPathToSubfolderInLocalDirectory("data"),
                PathToInterviewsDirectory = AndroidPathUtils.GetPathToSubfolderInLocalDirectory($"data{Path.DirectorySeparatorChar}interviews")
            });

            registry.Bind(typeof(IPlainStorage<,>), typeof(SqlitePlainStorage<,>));
            registry.Bind(typeof(IPlainStorage<>), typeof(SqlitePlainStorage<>));

            registry.BindAsSingleton<IPlainStorage<InterviewerIdentity>, SqlitePlainStorageWithoutWorkspace<InterviewerIdentity>>();
            registry.BindAsSingleton<IPlainStorage<WorkspaceView>, SqlitePlainStorageWithoutWorkspace<WorkspaceView>>();
            registry.Bind<IPlainStorage<PrefilledQuestionView>, PrefilledQuestionsRepository>();

            registry.Bind<INotificationsCollector, InterviewerNotificationsCollector>();
            
            registry.Bind<ICalendarEventStorage, CalendarEventStorage>();
            registry.Bind<ICalendarEventRemoval, CalendarEventRemoval>();
        }
    }
}
