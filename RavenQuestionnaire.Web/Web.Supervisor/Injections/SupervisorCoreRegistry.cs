// -----------------------------------------------------------------------
// <copyright file="MainCoreRegistry.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System.Reflection;
using Core.Supervisor.Synchronization;
using Main.Core;
using System.Linq;
using Main.Core.Events;
using Questionnaire.Core.Web.Security;

namespace Web.Supervisor.Injections
{
    using Questionnaire.Core.Web.Export;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class SupervisorCoreRegistry : CoreRegistry
    {
        public SupervisorCoreRegistry(string repositoryPath, bool isEmbeded)
            : base(repositoryPath, isEmbeded)
        {
        }
        public override System.Collections.Generic.IEnumerable<System.Reflection.Assembly> GetAssweblysForRegister()
        {
            return
                base.GetAssweblysForRegister().Concat(new Assembly[] {typeof(SupervisorEventSync).Assembly, typeof (QuestionnaireMembershipProvider).Assembly});
        }
        public override void Load()
        {
            base.Load();
            this.Unbind<IEventSync>();
            this.Bind<IEventSync>().To<SupervisorEventSync>();
            this.Unbind<IExportImport>();
            this.Bind<IExportImport>().To<ExportImportEvent>();
        }
    }
}
