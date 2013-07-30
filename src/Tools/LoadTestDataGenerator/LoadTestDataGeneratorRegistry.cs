using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Core.Supervisor.Denormalizer;
using Main.Core;
using WB.Core.Infrastructure.Raven.Implementation.ReadSide.RepositoryAccessors;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;

namespace LoadTestDataGenerator
{
    using System.Web.Configuration;
    using Ninject;
    using Ninject.Activation;

    public class LoadTestDataGeneratorRegistry : CoreRegistry
    {
        protected override IEnumerable<Assembly> GetAssembliesForRegistration()
        {
            return base.GetAssembliesForRegistration().Concat(new[]
            {
                typeof(LoadTestDataGeneratorRegistry).Assembly,
                typeof(CompleteQuestionnaireDenormalizer).Assembly,
                typeof(Questionnaire).Assembly,
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

        public static bool ShouldUsePersistentReadLayer()
        {
            return bool.Parse(WebConfigurationManager.AppSettings["ShouldUsePersistentReadLayer"]);
        }
    }
}

