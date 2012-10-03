// -----------------------------------------------------------------------
// <copyright file="MainCoreRegistry.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System.Linq;
using System.Reflection;
using Core.CAPI.Synchronization;
using Core.CAPI.Views;
using Main.Core;
using Main.Core.Events;
using Main.Core.View.CompleteQuestionnaire.ScreenGroup;
using Questionnaire.Core.Web.Security;

namespace Web.CAPI.Injections
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class CAPICoreRegistry : CoreRegistry
    {
        public CAPICoreRegistry(string repositoryPath, bool isEmbeded)
            : base(repositoryPath, isEmbeded)
        {
        }
        public override System.Collections.Generic.IEnumerable<System.Reflection.Assembly> GetAssweblysForRegister()
        {
            return
                base.GetAssweblysForRegister().Concat(new Assembly[] { typeof(ClientEventSync).Assembly, typeof(QuestionnaireMembershipProvider).Assembly });
        }
        public override void Load()
        {
            base.Load();
            this.Bind<IEventSync>().To<ClientEventSync>();
            this.Unbind<IScreenViewSupplier>();
            this.Bind<IScreenViewSupplier>().To<CapiScreenViewSupplier>();
        }
    }
}
