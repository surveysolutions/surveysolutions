// -----------------------------------------------------------------------
// <copyright file="MainCoreRegistry.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using Main.Core;

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
    }
}
