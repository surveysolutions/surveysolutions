// -----------------------------------------------------------------------
// <copyright file="MainCoreRegistry.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using Main.Core;
using Main.Core.Events;
using RavenQuestionnaire.Web.Synchronization;

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
        public override void Load()
        {
            base.Load();
            this.Bind<IEventSync>().To<HQEventSync>();
        }
    }
}
