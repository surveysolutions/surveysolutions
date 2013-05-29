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
    using Main.Synchronization.SycProcessRepository;

    using Questionnaire.Core.Web.Export.csv;
    using Questionnaire.Core.Web.Security;

    using WB.Core.SharedKernel.Logger;
    using WB.Core.SharedKernel.Utils.Compression;
    
    using Web.Supervisor.Filters;

    public class SupervisorCoreRegistry : CoreRegistry
    {
        private readonly bool isApprovedSended;

        public SupervisorCoreRegistry(string repositoryPath, bool isEmbeded, bool isApprovedSended)
            : base(repositoryPath, isEmbeded)
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

        protected override IEnumerable<KeyValuePair<Type, Type>> GetTypesForRegistration()
        {
            return base.GetTypesForRegistration().Concat(new Dictionary<Type, Type>
            {
                { typeof(IFilterProvider), typeof(RequiresReadLayerFilterProvider) },
                {typeof(IExceptionFilter), typeof(HandleUIExceptionAttribute)}
            });
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