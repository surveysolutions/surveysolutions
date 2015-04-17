using Main.Core.Documents;
using Ninject;
using Ninject.Modules;
using Sqo;

using WB.Core.Infrastructure.Implementation;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Storage.Mobile.Siaqodb;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.UI.QuestionnaireTester.Ninject
{
    public class PlainStorageInfrastructureModule : NinjectModule
    {
        private readonly string pathToStorage;

        public PlainStorageInfrastructureModule(string pathToStorage)
        {
            this.pathToStorage = pathToStorage;
        }

        public override void Load()
        {
            this.ConfigurePlainStorage();

            this.Bind<ISiaqodb>().ToConstant(new Siaqodb(this.pathToStorage));
            
            this.Bind(typeof (IPlainStorageAccessor<>)).To(typeof (SiaqodbPlainStorageAccessor<>)).InSingletonScope();
            this.Bind(typeof(IQueryablePlainStorageAccessor<>)).To(typeof(SiaqodbQueryablePlainStorageAccessor<>)).InSingletonScope();

            this.Bind<IPlainStorageAccessor<QuestionnaireDocument>>().To<InMemoryPlainStorageAccessor<QuestionnaireDocument>>().InSingletonScope();
            this.Bind<IPlainQuestionnaireRepository>().To<PlainQuestionnaireRepository>().InSingletonScope();
            this.Bind<IQuestionnaireRepository>().ToConstant<IQuestionnaireRepository>(this.Kernel.Get<IPlainQuestionnaireRepository>());

            this.Bind<IPlainInterviewRepository>().To<PlainInterviewRepository>().InSingletonScope();
            this.Bind<IPlainStorageAccessor<InterviewModel>>().To<InMemoryPlainStorageAccessor<InterviewModel>>().InSingletonScope();
        }

        private void ConfigurePlainStorage()
        {
            this.Bind<IDocumentSerializer>().To<SiaqodbPlainStorageSerializer>().InSingletonScope();
            SiaqodbConfigurator.SetDocumentSerializer(this.Kernel.Get<IDocumentSerializer>());
        }
    }
}