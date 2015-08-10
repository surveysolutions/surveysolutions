using System;
using System.IO;
using Cirrious.CrossCore.IoC;
using PCLStorage;
using WB.Core.BoundedContexts.Tester;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.Ncqrs;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator;
using WB.Infrastructure.Shared.Enumerator;
using WB.Infrastructure.Shared.Enumerator.Ninject;
using WB.UI.Shared.Enumerator;
using WB.UI.Shared.Enumerator.Ninject;
using WB.UI.Tester.Infrastructure;

namespace WB.UI.Tester.Ninject
{
    public class NinjectIoCAdapterSetup
    {
        public static IMvxIoCProvider CreateIocProvider()
        {
            var basePath = Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Personal))
               ? Environment.GetFolderPath(Environment.SpecialFolder.Personal)
               : Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;

            return new NinjectMvxIocProvider(

                new NcqrsModule().AsNinject(),
                new InfrastructureModuleMobile().AsNinject(),
                new DataCollectionInfrastructureModule(basePath).AsNinject(),

                new EnumeratorSharedKernelModule(),
                new EnumeratorInfrastructureModule(),

                new TesterInfrastructureModule(),
                new TesterUIModule());
        }
    }
}