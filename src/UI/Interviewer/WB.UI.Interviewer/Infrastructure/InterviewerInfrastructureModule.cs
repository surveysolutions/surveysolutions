using System.IO;
using System.Threading.Tasks;
using Main.Core.Documents;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
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
using IPrincipal = WB.Core.SharedKernels.Enumerator.Services.Infrastructure.IPrincipal;
using WB.UI.Shared.Enumerator.Services;
using WB.UI.Shared.Enumerator.Services.Internals;

namespace WB.UI.Interviewer.Infrastructure
{
    public class InterviewerInfrastructureModule : IModule
    {
        public void Load(IIocRegistry registry)
        {
            registry.BindToRegisteredInterface<IPrincipal, IInterviewerPrincipal>();
            registry.BindAsSingleton<IInterviewerPrincipal, InterviewerPrincipal>();
            registry.Bind<IStringCompressor, JsonCompressor>();

            var pathToLocalDirectory = AndroidPathUtils.GetPathToInternalDirectory();

            registry.BindAsSingletonWithConstructorArgument<IBackupRestoreService, BackupRestoreService>(
                "privateStorage", pathToLocalDirectory);

            registry.BindAsSingletonWithConstructorArgument<IQuestionnaireAssemblyAccessor, InterviewerQuestionnaireAssemblyAccessor>(
                "pathToAssembliesDirectory", AndroidPathUtils.GetPathToSubfolderInLocalDirectory("assemblies"));
            registry.Bind<ISerializer, PortableJsonSerializer>();
            registry.Bind<IInterviewAnswerSerializer, PortableInterviewAnswerJsonSerializer>();
            registry.Bind<IJsonAllTypesSerializer, PortableJsonAllTypesSerializer>();

            registry.BindAsSingleton<IPlainKeyValueStorage<QuestionnaireDocument>, QuestionnaireKeyValueStorage>();

            registry.Bind<IInterviewerQuestionnaireAccessor, InterviewerQuestionnaireAccessor>();
            registry.Bind<IInterviewerInterviewAccessor, InterviewerInterviewAccessor>();
            registry.Bind<IInterviewEventStreamOptimizer, InterviewEventStreamOptimizer>();
            registry.Bind<IQuestionnaireTranslator, QuestionnaireTranslator>();
            registry.BindAsSingleton<IQuestionnaireStorage, QuestionnaireStorage>();
            registry.Bind<IAudioFileStorage, InterviewerAudioFileStorage>();
            registry.Bind<IImageFileStorage, InterviewerImageFileStorage>();
            registry.Bind<IAnswerToStringConverter, AnswerToStringConverter>();
            registry.BindAsSingleton<IAssignmentDocumentsStorage, AssignmentDocumentsStorage>();
            registry.BindAsSingleton<IAuditLogService, EnumeratorAuditLogService>();

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

        public Task Init(IServiceLocator serviceLocator, UnderConstructionInfo status)
        {
            return Task.CompletedTask;
        }
    }
}
