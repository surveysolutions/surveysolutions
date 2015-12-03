using System;
using Main.Core.Documents;
using Ncqrs.Eventing.Storage;
using Ninject;
using Ninject.Modules;
using PCLStorage;
using Sqo;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Infrastructure.Shared.Enumerator;
using WB.UI.Interviewer.Infrastructure.Internals.Crasher.Data.Submit;
using WB.UI.Interviewer.Infrastructure.Logging;
using WB.UI.Interviewer.Settings;

namespace WB.UI.Interviewer.Infrastructure
{
    public class InterviewerInfrastructureModule : NinjectModule
    {
        private readonly string questionnaireAssembliesFolder;

        public InterviewerInfrastructureModule(string questionnaireAssembliesFolder = "assemblies")
        {
            this.questionnaireAssembliesFolder = questionnaireAssembliesFolder;
        }

        public override void Load()
        {
            this.Bind<IDocumentSerializer>().To<SiaqodbSerializer>();

            SiaqodbConfigurator.SetLicense(@"yrwPAibl/TwJ+pR5aBOoYieO0MbZ1HnEKEAwjcoqtdrUJVtXxorrxKZumV+Z48/Ffjj58P5pGVlYZ0G1EoPg0w==");
            SiaqodbConfigurator.SetDocumentSerializer(this.Kernel.Get<IDocumentSerializer>());
            SiaqodbConfigurator.AddDocument("Document", typeof(QuestionnaireDocumentView));
            SiaqodbConfigurator.AddDocument("Model", typeof(QuestionnaireModelView));
            SiaqodbConfigurator.AddText("JsonEvent", typeof(EventView));
            SiaqodbConfigurator.SpecifyStoredDateTimeKind(DateTimeKind.Utc);
            SiaqodbConfigurator.PropertyUseField("Id", "_id", typeof(IPlainStorageEntity));

            this.Bind<ISiaqodb>().ToConstant(new Siaqodb(AndroidPathUtils.GetPathToSubfolderInLocalDirectory("database")));
            
            this.Bind<IPlainKeyValueStorage<QuestionnaireModel>>().To<QuestionnaireModelKeyValueStorage>().InSingletonScope();
            this.Bind<IPlainKeyValueStorage<QuestionnaireDocument>>().To<QuestionnaireKeyValueStorage>().InSingletonScope();

            this.Bind(typeof(IAsyncPlainStorage<>)).To(typeof(SiaqodbPlainStorageWithCache<>)).InSingletonScope();
            this.Bind<IInterviewerQuestionnaireFactory>().To<InterviewerQuestionnaireFactory>();
            this.Bind<IInterviewerInterviewFactory>().To<InterviewerInterviewFactory>();
            this.Unbind<IPlainInterviewFileStorage>();
            this.Bind<IPlainInterviewFileStorage>().To<InterviewerPlainInterviewFileStorage>();

            this.Bind<IEventStore>().To<SiaqodbEventStorage>();

            this.Bind<IEnumeratorSettings>().To<InterviewerSettings>();

            this.Bind<InterviewerPrincipal>().To<InterviewerPrincipal>().InSingletonScope();
            this.Bind<IPrincipal>().ToMethod<IPrincipal>(context => context.Kernel.Get<InterviewerPrincipal>());
            this.Bind<IInterviewerPrincipal>().ToMethod<IInterviewerPrincipal>(context => context.Kernel.Get<InterviewerPrincipal>());

            var logFolderName = "logs";
            this.Bind<ILogger>().ToConstant(new FileLogger(AndroidPathUtils.GetPathToFileInLocalSubDirectory(logFolderName, "errors.log")));
            this.Bind<IReportSender>().ToConstant(new FileReportSender(AndroidPathUtils.GetPathToFileInLocalSubDirectory(logFolderName, "crashes.log")));

            this.Bind<ITroubleshootingService>().To<TroubleshootingService>();

            this.Bind<IQuestionnaireAssemblyFileAccessor>().ToConstructor(
                kernel => new InterviewerQuestionnaireAssemblyFileAccessor(kernel.Inject<IFileSystemAccessor>(),
                    AndroidPathUtils.GetPathToSubfolderInLocalDirectory("assemblies")));
        }
    }
}