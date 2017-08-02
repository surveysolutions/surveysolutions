using System;
using System.IO;
using Autofac;
using MvvmCross.Platform.IoC;
using WB.Core.BoundedContexts.Tester;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.Ncqrs;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator;
using WB.UI.Shared.Enumerator;
using WB.UI.Shared.Enumerator.Autofac;
using WB.UI.Shared.Enumerator.Services;
using WB.UI.Shared.Enumerator.Services.Internals;
using WB.UI.Tester.Infrastructure;

namespace WB.UI.Tester.ServiceLocation
{
    public class IoCAdapterSetup
    {
        public static IMvxIoCProvider CreateIocProvider()
        {
            var basePath = Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Personal))
               ? Environment.GetFolderPath(Environment.SpecialFolder.Personal)
               : AndroidPathUtils.GetPathToExternalDirectory();
            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterModule(new NcqrsModule().AsAutofac());
            builder.RegisterModule(new InfrastructureModuleMobile().AsAutofac());
            builder.RegisterModule(new DataCollectionSharedKernelModule().AsAutofac());
            builder.RegisterModule(new TesterBoundedContextModule().AsAutofac());
            builder.RegisterModule(new TesterInfrastructureModule(basePath).AsAutofac());
            builder.RegisterModule(new EnumeratorUIModule().AsAutofac());
            builder.RegisterModule(new TesterUIModule().AsAutofac());
            builder.RegisterModule(new EnumeratorSharedKernelModule().AsAutofac());

            var container = builder.Build();

            ServiceLocator.SetLocatorProvider(() => new AutofacServiceLocatorAdapter(container));
            return new MvxIoCProvider(container);
        }
    }
}