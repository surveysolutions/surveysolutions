// -----------------------------------------------------------------------
// <copyright file="AndroidCoreRegistry.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AndroidNcqrs.Eventing.Storage.SQLite;
using CAPI.Android.Core.Model;
using CAPI.Android.Core.Model.Authorization;
using CAPI.Android.Core.Model.ViewModel.Dashboard;
using Main.Core.View.User;
using Main.DenormalizerStorage;
using Ncqrs.Eventing.Storage;
using Ninject;
using Ninject.Activation;
using Main.Core;

namespace CAPI.Android.Injections
{
    public class AndroidCoreRegistry : CoreRegistry
    {
        protected override IEnumerable<Assembly> GetAssembliesForRegistration()
        {
            return
                Enumerable.Concat(base.GetAssembliesForRegistration(), new[] { GetType().Assembly });
        }
    }
}
