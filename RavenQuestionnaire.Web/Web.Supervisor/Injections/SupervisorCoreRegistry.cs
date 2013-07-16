using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Web.Configuration;
using Core.Supervisor.RavenIndexes;
using Core.Supervisor.Views.Index;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Indexes;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.Infrastructure.Raven.Implementation;
using WB.Core.Infrastructure.Raven.Implementation.ReadSide;
using WB.Core.Infrastructure.Raven.Implementation.ReadSide.RepositoryAccessors;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;
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
            return base.GetAssweblysForRegister().Concat(new[]
            {
                typeof(IndexViewFactory).Assembly,
                typeof(QuestionnaireMembershipProvider).Assembly,
                typeof(ImportQuestionnaireCommand).Assembly,
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

        private static bool ShouldUsePersistentReadLayer()
        {
            return bool.Parse(WebConfigurationManager.AppSettings["ShouldUsePersistentReadLayer"]);
        }

        public override void Load()
        {
            base.Load();

            this.Bind<IExportProvider<CompleteQuestionnaireExportView>>().To<CSVExporter>();
            this.Bind<IEnvironmentSupplier<CompleteQuestionnaireExportView>>().To<StataSuplier>();
            
            this.Bind<IStringCompressor>().ToConstant(new GZipJsonCompressor()).InSingletonScope();


            var store = Kernel.Get<DocumentStore>();
            var catalog = new CompositionContainer(new AssemblyCatalog(typeof(SummaryItemByTemplate).Assembly));
            IndexCreation.CreateIndexes(catalog, store.DatabaseCommands.ForDatabase("Views"), store.Conventions);
        }
    }
}