// -----------------------------------------------------------------------
// <copyright file="MainCoreRegistry.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using Main.Core;

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
    }
}
