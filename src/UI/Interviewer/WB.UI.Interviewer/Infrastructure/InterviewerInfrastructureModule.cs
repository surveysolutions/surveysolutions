using System;
using System.IO;
using Ninject;
using Ninject.Modules;
using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.UI.Interviewer.Infrastructure.Logging;
using IPrincipal = WB.Core.SharedKernels.Enumerator.Services.Infrastructure.IPrincipal;
using WB.UI.Interviewer.Services;
using WB.UI.Shared.Enumerator.Services;
using WB.UI.Shared.Enumerator.Services.Internals;
using ILogger = WB.Core.GenericSubdomains.Portable.Services.ILogger;

namespace WB.UI.Interviewer.Infrastructure
{
    public class InterviewerInfrastructureModule : NinjectModule
    {
        public override void Load()
        {
            this.Kernel.Load<InterviewerStorageModule>();

            this.Bind<IPrincipal>().ToMethod<IPrincipal>(context => context.Kernel.Get<InterviewerPrincipal>());
            this.Bind<IInterviewerPrincipal>().ToMethod<IInterviewerPrincipal>(context => context.Kernel.Get<InterviewerPrincipal>());
            this.Bind<InterviewerPrincipal>().To<InterviewerPrincipal>().InSingletonScope();

            this.Bind<IStringCompressor>().To<JsonCompressor>();

            var pathToLocalDirectory = AndroidPathUtils.GetPathToInternalDirectory();

            var fileName = Path.Combine(pathToLocalDirectory, "Logs", "${shortdate}.log");
            var fileTarget = new FileTarget("logFile")
            {
                FileName = fileName,
                Layout = "${longdate}[${logger}][${level}][${message}][${onexception:${exception:format=tostring}|${stacktrace}}]"
            };

            var config = new LoggingConfiguration();
            config.AddTarget("logFile", fileTarget);
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Warn, fileTarget));
            LogManager.Configuration = config;

            this.Bind<IBackupRestoreService>()
                .To<BackupRestoreService>()
                .WithConstructorArgument("privateStorage", pathToLocalDirectory);

            this.Bind<ILoggerProvider>().To<NLogLoggerProvider>();
            this.Bind<ILogger>().ToMethod(context =>
            {
                if (context.Request.Target != null)
                    return new NLogLogger(context.Request.Target.Member.DeclaringType);

                return new NLogLogger("UNKNOWN");
            });

            this.Bind<IQuestionnaireAssemblyAccessor>().ToConstructor(
                kernel => new InterviewerQuestionnaireAssemblyAccessor(kernel.Inject<IFileSystemAccessor>(),
                    kernel.Inject<ILogger>(),
                    AndroidPathUtils.GetPathToSubfolderInLocalDirectory("assemblies")));

            this.Bind<ISerializer>().ToMethod((ctx) => new PortableJsonSerializer());
            this.Bind<IInterviewAnswerSerializer>().ToMethod((ctx) => new PortableInterviewAnswerJsonSerializer());
            this.Bind<IJsonAllTypesSerializer>().ToMethod((ctx) => new PortableJsonAllTypesSerializer());
        }
    }
}