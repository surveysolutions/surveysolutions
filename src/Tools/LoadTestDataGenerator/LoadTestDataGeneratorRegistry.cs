using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Main.Core;

using WB.Core.Infrastructure.Raven.Implementation.ReadSide;

namespace LoadTestDataGenerator
{
    using System.Web.Configuration;

    using Main.DenormalizerStorage;

    using Ninject;
    using Ninject.Activation;

    using WB.Core.Infrastructure.Raven.Implementation;

    public class LoadTestDataGeneratorRegistry : CoreRegistry
    {
        public LoadTestDataGeneratorRegistry(string repositoryPath, bool isEmbeded)
            : base(repositoryPath, isEmbeded)
        {
        }

        public override IEnumerable<Assembly> GetAssweblysForRegister()
        {
            return
                base.GetAssweblysForRegister()
                    .Concat(new[]
                    {
                        typeof(LoadTestDataGeneratorRegistry).Assembly
                    });
        }

        protected override IEnumerable<KeyValuePair<Type, Type>> GetTypesForRegistration()
        {
            return base.GetTypesForRegistration().Concat(new Dictionary<Type, Type>
            {
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
            return bool.Parse(WebConfigurationManager.AppSettings["ShouldUsePersistentReadLayer"]);
        }
    }
}

