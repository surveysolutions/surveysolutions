using System.Web.Configuration;
using Core.Supervisor.Views.User;
using WB.Core.BoundedContexts.Supervisor.EventHandler;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.FunctionalDenormalization;
using WB.Core.Infrastructure.Raven.Implementation.ReadSide.RepositoryAccessors;
using WB.Core.SharedKernel.Utils.Serialization;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;
using WB.UI.Shared.Web.Filters;
using Web.Supervisor.Code;

namespace Web.Supervisor.Injections
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Web.Mvc;

    using Main.Core;
    using Ninject;
    using Ninject.Activation;
    using Questionnaire.Core.Web.Security;
    using WB.Core.SharedKernel.Utils.Compression;
    
    using Web.Supervisor.Filters;

    public class SupervisorCoreRegistry : CoreRegistry
    {
        protected override IEnumerable<Assembly> GetAssembliesForRegistration()
        {
            return base.GetAssembliesForRegistration().Concat(new[]
            {
                typeof(UserViewFactory).Assembly,
                typeof(QuestionnaireMembershipProvider).Assembly,
                typeof(ImportFromDesigner).Assembly,
                typeof(UserDenormalizer).Assembly
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

        protected override IEnumerable<KeyValuePair<Type, Type>> GetTypesForRegistration()
        {
            var supervisorSpecificTypes = new Dictionary<Type, Type>
            {
                { typeof(IExceptionFilter), typeof(HandleUIExceptionAttribute) }
            };

            if (!ShouldUsePersistentReadLayer())
            {
                supervisorSpecificTypes.Add(typeof(IFilterProvider), typeof(RequiresReadLayerFilterProvider));
            }

            return base.GetTypesForRegistration().Concat(supervisorSpecificTypes);
        }
        protected override void RegisterEventHandlers()
        {
            base.RegisterEventHandlers();

            BindInterface(this.GetAssembliesForRegistration(), typeof(IEventHandler), (c) => this.Kernel);
        }
        private static bool ShouldUsePersistentReadLayer()
        {
            return bool.Parse(WebConfigurationManager.AppSettings["ShouldUsePersistentReadLayer"]);
        }

        public override void Load()
        {
            base.Load();

            RegisterViewFactories();

            this.Bind<IJsonUtils>().To<NewtonJsonUtils>();
            this.Bind<IStringCompressor>().To<GZipJsonCompressor>();
            this.Bind<IRevalidateInterviewsAdministrationService>().To<RevalidateInterviewsAdministrationService>().InSingletonScope();
        }
    }
}