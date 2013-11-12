// -----------------------------------------------------------------------
// <copyright file="AndroidCoreRegistry.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Main.Core;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;

namespace WB.UI.Capi.Injections
{
    public class AndroidCoreRegistry : CoreRegistry
    {
        protected override IEnumerable<Assembly> GetAssembliesForRegistration()
        {
            return
                Enumerable.Concat(base.GetAssembliesForRegistration(), new[] { typeof(ImportFromSupervisor).Assembly, this.GetType().Assembly });
        }
    }
}
