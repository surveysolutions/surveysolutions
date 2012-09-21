// -----------------------------------------------------------------------
// <copyright file="MainCoreRegistry.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using Core.HQ.Synchronization;
using Main.Core;
using Main.Core.Events;

namespace RavenQuestionnaire.Web.Injections
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class HQCoreRegistry : CoreRegistry
    {
        public HQCoreRegistry(string repositoryPath, bool isEmbeded)
            : base(repositoryPath, isEmbeded)
        {
        }
        public override void Load()
        {
            base.Load();
            this.Bind<IEventSync>().To<HQEventSync>();
        }
    }
}
