using System;
using System.IO;
using MvvmCross.IoC;
using WB.Core.BoundedContexts.Tester;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.Modularity.Autofac;
using WB.Core.Infrastructure.Ncqrs;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator;
using WB.UI.Shared.Enumerator;
using WB.UI.Shared.Enumerator.Services;
using WB.UI.Tester.Infrastructure;
using MvxIoCProvider = WB.UI.Shared.Enumerator.Autofac.MvxIoCProvider;

namespace WB.UI.Tester.ServiceLocation
{
    public class IoCAdapterSetup
    {
        public static IMvxIoCProvider CreateIocProvider()
        {
            var basePath = Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Personal))
               ? Environment.GetFolderPath(Environment.SpecialFolder.Personal)
               : AndroidPathUtils.GetPathToExternalDirectory();

            AutofacKernel kernel = new AutofacKernel();
            
            kernel.Load(
                new NcqrsModule(),
                new InfrastructureModuleMobile(),
                new DataCollectionSharedKernelModule(),
                new TesterBoundedContextModule(),
                new TesterInfrastructureModule(basePath),
                new EnumeratorUIModule(),
                new TesterUIModule(),
                new EnumeratorSharedKernelModule()
                );

            kernel.Init().Wait();

            return new MvxIoCProvider(kernel.Container);
        }
    }
}
