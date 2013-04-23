// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SupervisorCoreRegistry.cs" company="">
//   
// </copyright>
// <summary>
//   TODO: Update summary.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Web.Supervisor.Injections
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using Core.Supervisor.Synchronization;

    using DataEntryClient.SycProcessFactory;

    using Main.Core;
    using Main.Core.Events;
    using Main.Core.Export;
    using Main.Core.View.Export;
    using Main.Synchronization.SycProcessRepository;

    using Questionnaire.Core.Web.Export.csv;
    using Questionnaire.Core.Web.Security;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class SupervisorCoreRegistry : CoreRegistry
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SupervisorCoreRegistry"/> class.
        /// </summary>
        /// <param name="repositoryPath">
        /// The repository path.
        /// </param>
        /// <param name="isEmbeded">
        /// The is embeded.
        /// </param>
        public SupervisorCoreRegistry(string repositoryPath, bool isEmbeded)
            : base(repositoryPath, isEmbeded)
        {
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The get assweblys for register.
        /// </summary>
        /// <returns>
        /// List of assemblies
        /// </returns>
        public override IEnumerable<Assembly> GetAssweblysForRegister()
        {
            return
                base.GetAssweblysForRegister().Concat(
                    new[]
                    {
                            typeof(SupervisorEventStreamReader).Assembly, typeof(QuestionnaireMembershipProvider).Assembly
                    });
        }

        /// <summary>
        /// The load.
        /// </summary>
        public override void Load()
        {
            base.Load();
            this.Unbind<IEventStreamReader>();
            this.Bind<IEventStreamReader>().To<SupervisorEventStreamReader>();

            this.Bind<IExportProvider<CompleteQuestionnaireExportView>>().To<CSVExporter>();
            this.Bind<IEnvironmentSupplier<CompleteQuestionnaireExportView>>().To<StataSuplier>();

            this.Bind<ISyncProcessRepository>().To<SyncProcessRepository>();
            this.Bind<ISyncProcessFactory>().To<SyncProcessFactory>();
        }

        #endregion
    }
}