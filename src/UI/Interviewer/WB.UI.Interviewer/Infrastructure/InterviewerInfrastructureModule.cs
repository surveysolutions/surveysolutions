using Main.Core.Documents;
using Ninject;
using Ninject.Modules;
using PCLStorage;
using Sqo;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.Enumerator;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
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
            this.Bind<IDocumentSerializer>().To<SiaqodbSerializer>();

            SiaqodbConfigurator.SetLicense(@"yrwPAibl/TwJ+pR5aBOoYieO0MbZ1HnEKEAwjcoqtdrUJVtXxorrxKZumV+Z48/Ffjj58P5pGVlYZ0G1EoPg0w==");
            SiaqodbConfigurator.SetDocumentSerializer(this.Kernel.Get<IDocumentSerializer>());
            SiaqodbConfigurator.AddDocument("Document", typeof(QuestionnaireDocumentView));
            SiaqodbConfigurator.AddDocument("Model", typeof(QuestionnaireModelView));

            this.Bind<ISiaqodb>().ToConstant(new Siaqodb(AndroidPathUtils.GetPathToSubfolderInLocalDirectory("database")));
            
            this.Bind<IPlainKeyValueStorage<QuestionnaireModel>>().To<QuestionnaireModelKeyValueStorage>().InSingletonScope();
            this.Bind<IPlainKeyValueStorage<QuestionnaireDocument>>().To<QuestionnaireKeyValueStorage>().InSingletonScope();
            //this.Bind<IReadSideKeyValueStorage<QuestionnaireRosterStructure>>().ToConstant(propagationStructureStore);

            this.Bind(typeof(IAsyncPlainStorage<>)).To(typeof(SiaqodbPlainStorageWithCache<>)).InSingletonScope();

            this.Bind<IEnumeratorSettings>().To<InterviewerSettings>();

            this.Bind<InterviewerPrincipal>().To<InterviewerPrincipal>().InSingletonScope();
            this.Bind<IPrincipal>().ToMethod<IPrincipal>(context => context.Kernel.Get<InterviewerPrincipal>());
            this.Bind<IInterviewerPrincipal>().ToMethod<IInterviewerPrincipal>(context => context.Kernel.Get<InterviewerPrincipal>());

            this.Bind<ICapiDataSynchronizationService>().To<CapiDataSynchronizationService>();
            this.Bind<IQuestionnaireAssemblyFileAccessor>()
                .To<InterviewerQuestionnaireAssemblyFileAccessor>().InSingletonScope().WithConstructorArgument("folderPath", FileSystem.Current.LocalStorage.Path).WithConstructorArgument("assemblyDirectoryName", this.questionnaireAssembliesFolder);
        }
    }
}