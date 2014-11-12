// -----------------------------------------------------------------------
// <copyright file="AndroidCoreRegistry.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Main.Core;
using WB.Core.BoundedContexts.Capi.Views.Login;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernel.Utils.Compression;
using WB.Core.SharedKernel.Utils.Serialization;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;
using WB.UI.Capi.Views.Login;

namespace WB.UI.Capi.Injections
{
    public class AndroidCoreRegistry : CoreRegistry
    {
        protected override IEnumerable<Assembly> GetAssembliesForRegistration()
        {
            return
                Enumerable.Concat(base.GetAssembliesForRegistration(), new[] { typeof(ImportFromSupervisor).Assembly, this.GetType().Assembly });
        }

        public override void Load()
        {
            base.Load();
            
            this.Bind<IJsonUtils>().To<NewtonJsonUtils>();
            this.Bind<IStringCompressor>().To<GZipJsonCompressor>();
            this.Bind<IViewFactory<LoginViewInput, LoginView>>().To<LoginViewFactory>();
        }
    }
}
