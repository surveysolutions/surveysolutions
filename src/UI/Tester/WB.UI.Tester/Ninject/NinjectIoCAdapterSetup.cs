using System;
using System.IO;
using MvvmCross.Platform.IoC;
using WB.Core.BoundedContexts.Tester;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.Ncqrs;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator;
using WB.UI.Shared.Enumerator;
using WB.UI.Shared.Enumerator.Ninject;
using WB.UI.Shared.Enumerator.Services;
using WB.UI.Shared.Enumerator.Services.Ninject;
using WB.UI.Tester.Infrastructure;

namespace WB.UI.Tester.Ninject
{
    public class NinjectIoCAdapterSetup
    {
        public static IMvxIoCProvider CreateIocProvider()
        {
            var basePath = Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Personal))
               ? Environment.GetFolderPath(Environment.SpecialFolder.Personal)
               : AndroidPathUtils.GetPathToExternalDirectory();

            return new NinjectMvxIocProvider(

                new NcqrsModule().AsNinject(),
                new InfrastructureModuleMobile().AsNinject(),

                new DataCollectionSharedKernelModule().AsNinject(),
                new EnumeratorSharedKernelModule(),

                new TesterBoundedContextModule().AsNinject(),
                new TesterInfrastructureModule(basePath),
                new EnumeratorUIModule(),
                new TesterUIModule());
        }
    }
}