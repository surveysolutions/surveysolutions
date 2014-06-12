// -----------------------------------------------------------------------
// <copyright file="AndroidCoreRegistry.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CAPI.Android.Core.Model;
using Main.Core;
using Main.Core.View;
using WB.Core.BoundedContexts.Capi.Synchronization.Views.InterviewMetaInfo;
using WB.Core.BoundedContexts.Capi.Synchronization.Views.Login;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernel.Utils.Serialization;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;
using WB.UI.Capi.Syncronization;
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
            this.Bind<IViewFactory<LoginViewInput, LoginView>>().To<LoginViewFactory>();
        }
    }
}
