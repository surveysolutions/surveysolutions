using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonServiceLocator.NinjectAdapter;
using Main.Core;
using Main.Core.Documents;
using Main.Core.ExpressionExecutors;
using Main.DenormalizerStorage;
using Microsoft.Practices.ServiceLocation;
using Ncqrs;
using Ncqrs.Eventing.Storage;
using Ncqrs.Eventing.Storage.RavenDB;
using Ninject;
using Ninject.Modules;
using Raven.Client.Document;
using WB.Core.BoundedContexts.Designer;
using WB.Core.GenericSubdomains.Logging.NLog;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.Raven;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
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
                new SynchronizationModule(System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName)),
                new NLogLoggingModule(),
                new DesignerBoundedContextModule()
            );
            
            RegisterServices(kernel);

            return kernel;
        }

        private static void RegisterServices(IKernel kernel)
        {
            ServiceLocator.SetLocatorProvider(() => new NinjectServiceLocator(kernel));

            kernel.Bind<IServiceLocator>().ToMethod(_ => ServiceLocator.Current);

            ConditionExecuterFactory.Creator = doc => new FakeCompleteQuestionnaireConditionExecuteCollector();

            kernel.Load(new LoadTestDataGeneratorRegistry(repositoryPath: ConfigurationManager.AppSettings["Raven.DocumentStore"], isEmbeded: false));

            NcqrsInit.InitializeEventStore = (store, pageSize) => new BatchedRavenDBEventStore(store, pageSize);

            NcqrsInit.Init(kernel);
            
            kernel.Load<MainModule>();
        }
    }
}
