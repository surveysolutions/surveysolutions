using System.IO;
using System.Web.Configuration;
using Core.Supervisor.Views.Index;
using WB.Core.Infrastructure.Raven.Implementation;
using WB.Core.SharedKernel.Utils.Logging;
using WB.Core.Synchronization.ImportManager;
using WB.Core.Synchronization.SyncManager;
using WB.Core.Synchronization.SyncProvider;
using WB.Core.Synchronization.SyncStorage;
using WB.UI.Shared.Web.Filters;

namespace Web.Supervisor.Injections
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Web.Mvc;

    using Main.Core;
    using Main.Core.Events;
    using Main.Core.Export;
    using Main.Core.View.Export;
    using Main.DenormalizerStorage;

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
                            typeof(IndexViewFactory).Assembly, typeof(QuestionnaireMembershipProvider).Assembly
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

            this.Bind<IExportProvider<CompleteQuestionnaireExportView>>().To<CSVExporter>();
            this.Bind<IEnvironmentSupplier<CompleteQuestionnaireExportView>>().To<StataSuplier>();

            this.Bind<ILog>().ToMethod(
                context => LogManager.GetLogger(context.Request.Target.Member.DeclaringType));

            this.Bind<IStringCompressor>().ToConstant(new GZipJsonCompressor()).InSingletonScope();

            this.Bind<ISyncManager>().To<SyncManager>();
            this.Bind<ISyncProvider>().To<SyncProvider>();
            this.Bind<IImportManager>().To<DefaultImportManager>();
           
            this.Bind<IChunkStorage>()
                .To<FileChunkStorage>()
                .WithConstructorArgument("folderPath",  AppDomain.CurrentDomain.GetData("DataDirectory").ToString());
            this.Bind<ISynchronizationDataStorage>().To<SimpleSynchronizationDataStorage>();

        }
    }
}