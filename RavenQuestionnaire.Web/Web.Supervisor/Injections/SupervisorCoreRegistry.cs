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

    using WB.UI.Shared.Log;
    using WB.UI.Shared.NLog;

    using Web.Supervisor.Filters;

    public class SupervisorCoreRegistry : CoreRegistry
    {
        public SupervisorCoreRegistry(string repositoryPath, bool isEmbeded)
            : base(repositoryPath, isEmbeded) {}

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
            });
        }

        public override void Load()
        {
            base.Load();

            this.Unbind<IEventStreamReader>();
            this.Bind<IEventStreamReader>().To<SupervisorEventStreamReader>();

            this.Bind<IExportProvider<CompleteQuestionnaireExportView>>().To<CSVExporter>();
            this.Bind<IEnvironmentSupplier<CompleteQuestionnaireExportView>>().To<StataSuplier>();

            this.Bind<ISyncProcessRepository>().To<SyncProcessRepository>();
            this.Bind<ISyncProcessFactory>().To<SyncProcessFactory>();

            this.Bind<ILog>().ToConstant(new Log()).InSingletonScope();
        }
    }
}