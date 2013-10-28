using System.Configuration;
using Core.Supervisor.Views.User;
using Main.Core;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ninject;
using Ninject.Activation;
using WB.Core.Infrastructure.Raven.Implementation.ReadSide.RepositoryAccessors;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;
using UserDenormalizer = WB.Core.BoundedContexts.Supervisor.EventHandler.UserDenormalizer;

namespace CapiDataGenerator
{
    public class CapiDataGeneratorRegistry : CoreRegistry
    {
        protected override IEnumerable<Assembly> GetAssembliesForRegistration()
        {
            return base.GetAssembliesForRegistration().Concat(new[]
            {
                GetType().Assembly,
                typeof(UserDenormalizer).Assembly,
                typeof(ImportFromDesigner).Assembly,
                typeof(UserListViewFactory).Assembly
            });
        }

        protected override object GetReadSideRepositoryReader(IContext context)
        {
            return ShouldUsePersistentReadLayer()
                ? this.Kernel.Get(typeof(RavenReadSideRepositoryReader<>).MakeGenericType(context.GenericArguments[0]))
                : this.GetInMemoryReadSideRepositoryAccessor(context);
        }

        protected override object GetReadSideRepositoryWriter(IContext context)
        {
            return ShouldUsePersistentReadLayer()
                ? this.Kernel.Get(typeof(RavenReadSideRepositoryWriter<>).MakeGenericType(context.GenericArguments[0]))
                : this.GetInMemoryReadSideRepositoryAccessor(context);
        }

        private static bool ShouldUsePersistentReadLayer()
        {
            return bool.Parse(ConfigurationManager.AppSettings["ShouldUsePersistentReadLayer"]);
        }
    }
}
