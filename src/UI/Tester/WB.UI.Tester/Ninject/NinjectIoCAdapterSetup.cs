using System;
using System.IO;
using Cirrious.CrossCore.IoC;
using PCLStorage;
using WB.Core.BoundedContexts.Tester;
using WB.Core.BoundedContexts.Tester.Infrastructure;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.Ncqrs;
using WB.Infrastructure.Shared.Enumerator.Ninject;
using WB.UI.Shared.Enumerator.Ninject;

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

                new NcqrsModule().AsNinject(), // verified as shared
                new InfrastructureModuleMobile().AsNinject(), // verified as shared

                new EnumeratorSharedKernelModule(), // verified as shared
                new EnumeratorInfrastructureModule(basePath), // verified as shared
                new EnumeratorUIModule(), // verified as shared

                new TesterInfrastructureModule(), // verified as specific
                new TesterUIModule()); // verified as specific
        }
    }
}