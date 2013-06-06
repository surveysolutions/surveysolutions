using System.Web.Configuration;

using WB.Core.SharedKernel.Utils.Logging;
using WB.UI.Shared.Web.Filters;

namespace Web.Supervisor.Injections
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Web.Mvc;

    using Core.Supervisor.Synchronization;

    using DataEntryClient.SycProcessFactory;

    using Main.Core;
    using Main.Core.Events;
    using Main.Core.Export;
    using Main.Core.View.Export;
    using Main.DenormalizerStorage;
    using Main.Synchronization.SycProcessRepository;

    using Ninject;
    using Ninject.Activation;

    using Questionnaire.Core.Web.Export.csv;
    using Questionnaire.Core.Web.Security;

    using WB.Core.SharedKernel.Logger;
    using WB.Core.SharedKernel.Utils.Compression;
    
    using Web.Supervisor.Filters;

    public class SupervisorCoreRegistry : CoreRegistry
    {
        private readonly bool isApprovedSended;

        public SupervisorCoreRegistry(string repositoryPath, string defaultDatabase, bool isEmbeded, string username, string password, bool isApprovedSended)
            : base(repositoryPath, isEmbeded, username, password, defaultDatabase)
        {
            this.isApprovedSended = isApprovedSended;
        }

        public override IEnumerable<Assembly> GetAssweblysForRegister()
        {
            return
                base.GetAssweblysForRegister().Concat(
                    new[]
                    {
                            typeof(SupervisorEventStreamReader).Assembly, typeof(QuestionnaireMembershipProvider).Assembly
                    });
        }

        protected override object GetStorage(IContext context)
        {
            Type storageType = ShouldUsePersistentReadLayer()
                ? typeof(RavenDenormalizerStorage<>).MakeGenericType(context.GenericArguments[0])
                : typeof(InMemoryDenormalizer<>).MakeGenericType(context.GenericArguments[0]);

            return this.Kernel.Get(storageType);
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

        private static bool ShouldUsePersistentReadLayer()
        {
            return bool.Parse(WebConfigurationManager.AppSettings["ShouldUsePersistentReadLayer"]);
        }

        public override void Load()
        {
            base.Load();

            this.Unbind<IEventStreamReader>();
            this.Bind<IEventStreamReader>()
                .To<SupervisorEventStreamReader>()
                .WithConstructorArgument("isApprovedSended", isApprovedSended);

            this.Bind<IExportProvider<CompleteQuestionnaireExportView>>().To<CSVExporter>();
            this.Bind<IEnvironmentSupplier<CompleteQuestionnaireExportView>>().To<StataSuplier>();

            this.Bind<ISyncProcessRepository>().To<SyncProcessRepository>();
            this.Bind<ISyncProcessFactory>().To<SyncProcessFactory>();

            this.Bind<ILog>().ToMethod(
                context => LogManager.GetLogger(context.Request.Target.Member.DeclaringType));

            this.Bind<IStringCompressor>().ToConstant(new GZipJsonCompressor()).InSingletonScope();
        }
    }
}