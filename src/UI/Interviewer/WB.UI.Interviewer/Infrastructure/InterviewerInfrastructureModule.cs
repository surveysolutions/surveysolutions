using Ninject;
using Ninject.Modules;
using PCLStorage;
using Sqo;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.SharedKernels.DataCollection.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.Enumerator;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Infrastructure.Shared.Enumerator;
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
            SiaqodbConfigurator.SetLicense(@"yrwPAibl/TwJ+pR5aBOoYieO0MbZ1HnEKEAwjcoqtdrUJVtXxorrxKZumV+Z48/Ffjj58P5pGVlYZ0G1EoPg0w==");
            this.Bind<ISiaqodb>().ToConstant(new Siaqodb(AndroidPathUtils.GetPathToSubfolderInLocalDirectory("database")));

            this.Bind(typeof(IAsyncPlainStorage<>)).To(typeof(SiaqodbPlainStorage<>)).InSingletonScope();

            this.Bind<IEnumeratorSettings>().To<InterviewerSettings>();

            this.Bind<InterviewerPrincipal>().To<InterviewerPrincipal>().InSingletonScope();
            this.Bind<IPrincipal>().ToMethod<IPrincipal>(context => context.Kernel.Get<InterviewerPrincipal>());
            this.Bind<IInterviewerPrincipal>().ToMethod<IInterviewerPrincipal>(context => context.Kernel.Get<InterviewerPrincipal>());

            this.Bind<ICapiDataSynchronizationService>().To<CapiDataSynchronizationService>();
            this.Bind<IQuestionnaireAssemblyFileAccessor>()
                .To<QuestionnaireAssemblyFileAccessor>().InSingletonScope().WithConstructorArgument("folderPath", FileSystem.Current.LocalStorage.Path).WithConstructorArgument("assemblyDirectoryName", this.questionnaireAssembliesFolder);
        }
    }
}