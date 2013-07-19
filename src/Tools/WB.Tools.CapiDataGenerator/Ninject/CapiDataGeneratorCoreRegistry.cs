// -----------------------------------------------------------------------
// <copyright file="AndroidCoreRegistry.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using Main.Core;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CapiDataGenerator
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class CapiDataGeneratorRegistry : CoreRegistry
    {
        public CapiDataGeneratorRegistry(string repositoryPath, bool isEmbeded)
            : base(repositoryPath, isEmbeded)
        {
        }
        /// <summary>
        /// The get assweblys for register.
        /// </summary>
        /// <returns>
        /// </returns>
        public override IEnumerable<Assembly> GetAssweblysForRegister()
        {
            return
                Enumerable.Concat(base.GetAssweblysForRegister(), new[] { GetType().Assembly });
        }
    }
}
