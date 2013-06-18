using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core;
using Main.Core.Documents;
using Main.DenormalizerStorage;
using Ninject;
using Ninject.Modules;

using WB.Core.Infrastructure;
using WB.Core.Infrastructure.Raven;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Questionnaire.ExportServices;

namespace LoadTestDataGenerator
{
    public class CompositionRoot
    {
        private static StandardKernel kernel;

        public static StandardKernel Wire(INinjectModule module)
        {
            kernel = new StandardKernel(new RavenInfrastructureModule());
            
            RegisterServices(kernel);

            return kernel;
        }

        private static void RegisterServices(IKernel kernel)
        {
            kernel.Load(new LoadTestDataGeneratorRegistry(repositoryPath: ConfigurationManager.AppSettings["Raven.DocumentStore"], isEmbeded: false));

            NcqrsInit.Init(kernel);

            kernel.Bind<IExportService>().ToConstant(new JsonExportService(kernel.Get<IReadSideRepositoryReader<QuestionnaireDocument>>()));

            kernel.Load<MainModule>();
        }
    }
}
