// -----------------------------------------------------------------------
// <copyright file="MainCoreRegistry.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System.Linq;
using System.Reflection;
using Core.HQ.Synchronization;
using Main.Core;
using Main.Core.Events;
using Questionnaire.Core.Web.Security;
using RavenQuestionnaire.Core.Views.Questionnaire;

namespace RavenQuestionnaire.Web.Injections
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class MainCoreRegistry : CoreRegistry
    {
        public MainCoreRegistry(string repositoryPath, bool isEmbeded) : base(repositoryPath, isEmbeded)
        {
        }
        public override System.Collections.Generic.IEnumerable<System.Reflection.Assembly> GetAssweblysForRegister()
        {
            return
                base.GetAssweblysForRegister().Concat(new Assembly[] { typeof(HQEventSync).Assembly, typeof(QuestionnaireView).Assembly, typeof(QuestionnaireMembershipProvider).Assembly });
        }
        public override void Load()
        {
            base.Load();
            this.Bind<IEventSync>().To<HQEventSync>();
        }
    }
}
