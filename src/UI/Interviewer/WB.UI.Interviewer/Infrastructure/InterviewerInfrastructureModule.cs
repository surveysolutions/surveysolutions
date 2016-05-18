using System.Linq;
using Main.Core.Documents;
using Ncqrs.Eventing.Storage;
using Ninject;
using Ninject.Modules;
using NLog;
using NLog.Targets;
using SQLite.Net.Interop;
using SQLite.Net.Platform.XamarinAndroid;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Implementation.Storage;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Infrastructure.Shared.Enumerator;
using WB.UI.Interviewer.Infrastructure.Logging;
using IPrincipal = WB.Core.SharedKernels.Enumerator.Services.Infrastructure.IPrincipal;
using WB.Infrastructure.Shared.Enumerator.Internals;
using ILogger = WB.Core.GenericSubdomains.Portable.Services.ILogger;

namespace WB.UI.Interviewer.Infrastructure
{
    public class InterviewerInfrastructureModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IPlainKeyValueStorage<QuestionnaireDocument>>().To<QuestionnaireKeyValueStorage>().InSingletonScope();

 			this.Bind<IInterviewerQuestionnaireAccessor>().To<InterviewerQuestionnaireAccessor>();
            this.Bind<IInterviewerInterviewAccessor>().To<InterviewerInterviewAccessor>();
            this.Bind<IInterviewEventStreamOptimizer>().To<InterviewEventStreamOptimizer>();
            
            this.Bind<IPlainQuestionnaireRepository>().To<PlainQuestionnaireRepositoryWithCache>().InSingletonScope();
            this.Bind<IPlainInterviewFileStorage>().To<InterviewerPlainInterviewFileStorage>();

            this.Bind<IInterviewerEventStorage, IEventStore>()
                .To<SqliteEventStorage>()
                .InSingletonScope()
                .WithConstructorArgument("traceListener", new MvxTraceListener("EventStore-SQL-Queries"));
            
            this.Bind<ISQLitePlatform>().To<SQLitePlatformAndroid>();
            this.Bind<SqliteSettings>().ToConstant(
                new SqliteSettings()
                {
                    PathToDatabaseDirectory = AndroidPathUtils.GetPathToSubfolderInLocalDirectory("data")
                });
            this.Bind(typeof (IAsyncPlainStorage<>), typeof(IAsyncPlainStorageRemover<>)).To(typeof (SqlitePlainStorage<>)).InSingletonScope();

            this.Bind<InterviewerPrincipal>().To<InterviewerPrincipal>().InSingletonScope();
            this.Bind<IPrincipal>().ToMethod<IPrincipal>(context => context.Kernel.Get<InterviewerPrincipal>());
            this.Bind<IInterviewerPrincipal>().ToMethod<IInterviewerPrincipal>(context => context.Kernel.Get<InterviewerPrincipal>());

            var logFilePath = AndroidPathUtils.GetPathToLogFile();
            LogManager.Configuration.AllTargets.OfType<FileTarget>().ForEach(aTarget => aTarget.FileName = logFilePath);
            this.Bind<ILoggerProvider>().To<NLogLoggerProvider>();
            this.Bind<ILogger>().ToMethod(context =>
            {
                if (context.Request.Target != null)
                    return new NLogLogger(context.Request.Target.Member.DeclaringType);

                return new NLogLogger("UNKNOWN");
            });

            this.Bind<IBackupRestoreService>()
                .To<BackupRestoreService>()
                .WithConstructorArgument("privateStorage", AndroidPathUtils.GetPathToLocalDirectory())
                .WithConstructorArgument("logFilePath", AndroidPathUtils.GetPathToLogFile());

            this.Bind<IQuestionnaireAssemblyFileAccessor>().ToConstructor(
                kernel => new InterviewerQuestionnaireAssemblyFileAccessor(kernel.Inject<IFileSystemAccessor>(), 
                kernel.Inject<IAsynchronousFileSystemAccessor>(), kernel.Inject<ILogger>(), 
                AndroidPathUtils.GetPathToSubfolderInLocalDirectory("assemblies")));
            
            this.Bind<ISerializer>().ToMethod((ctx) => new PortableJsonSerializer());
            this.Bind<IJsonAllTypesSerializer>().ToMethod((ctx) => new PortableJsonAllTypesSerializer());

            this.Bind<IStringCompressor>().To<JsonCompressor>();
        }
    }
}