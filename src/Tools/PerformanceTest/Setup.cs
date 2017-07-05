using Autofac;
using Autofac.Extras.CommonServiceLocator;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Infrastructure.Native.Files.Implementation.FileSystem;

namespace PerformanceTest
{
    internal static class Setup
    {
        public static void MockedServiceLocator()
        {
            var cont = new ContainerBuilder();
            cont.RegisterType<RoslynExpressionProcessor>().As<IExpressionProcessor>().SingleInstance();

            cont.RegisterType<FileSystemIOAccessor>().As<IFileSystemAccessor>().SingleInstance();
            cont.RegisterType<Mocks.LookupTableServiceStub>().As<ILookupTableService>();

            cont.RegisterType<MacrosSubstitutionService>().As<IMacrosSubstitutionService>();

            cont.RegisterType<ExpressionsPlayOrderProvider>().As<IExpressionsPlayOrderProvider>();

            var kernel = cont.Build();
            
            // Set the service locator to an AutofacServiceLocator.
            var csl = new AutofacServiceLocator(kernel);
            ServiceLocator.SetLocatorProvider(() => csl);
        }
    }
}