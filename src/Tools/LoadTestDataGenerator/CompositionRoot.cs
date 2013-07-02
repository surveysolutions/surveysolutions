using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core;
using Main.Core.Documents;
using Main.DenormalizerStorage;
using Ncqrs;
using Ncqrs.Eventing.Storage;
using Ncqrs.Eventing.Storage.RavenDB;
using Ninject;
using Ninject.Modules;
using Raven.Client.Document;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.Raven;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Questionnaire.ExportServices;
using WB.Core.Synchronization;

namespace LoadTestDataGenerator
{
    public class CompositionRoot
    {
        private static StandardKernel kernel;

        public static StandardKernel Wire(INinjectModule module)
        {
            kernel = new StandardKernel(
                new NinjectSettings { InjectNonPublic = true },
                new RavenInfrastructureModule(),
                new SynchronizationModule());
            
            RegisterServices(kernel);

            return kernel;
        }

        private static void RegisterServices(IKernel kernel)
        {
            kernel.Load(new LoadTestDataGeneratorRegistry(repositoryPath: ConfigurationManager.AppSettings["Raven.DocumentStore"], isEmbeded: false));

            NcqrsInit.InitializeEventStore = (store, pageSize) => new BatchedRavenDBEventStore(store, pageSize);

            NcqrsInit.Init(kernel);

            kernel.Bind<IExportService>().ToConstant(new JsonExportService(kernel.Get<IReadSideRepositoryReader<QuestionnaireDocument>>()));

            
            kernel.Load<MainModule>();
        }
    }
}
