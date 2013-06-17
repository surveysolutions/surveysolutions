using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Main.Core;

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

        protected override object GetStorage(IContext context)
        {
            Type storageType = ShouldUsePersistentReadLayer()
                ? typeof(RavenDenormalizerStorage<>).MakeGenericType(context.GenericArguments[0])
                : typeof(InMemoryReadSideRepositoryAccessor<>).MakeGenericType(context.GenericArguments[0]);

            return this.Kernel.Get(storageType);
        }

        private static bool ShouldUsePersistentReadLayer()
        {
            return bool.Parse(WebConfigurationManager.AppSettings["ShouldUsePersistentReadLayer"]);
        }
    }
}

