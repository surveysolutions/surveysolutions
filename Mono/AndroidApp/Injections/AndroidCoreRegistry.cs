// -----------------------------------------------------------------------
// <copyright file="AndroidCoreRegistry.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System.Reflection;
using AndroidNcqrs.Eventing.Storage.SQLite;
using Core.CAPI.Synchronization;
using Main.Core;
using Ncqrs.Eventing.Storage;

namespace AndroidApp.Injections
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class AndroidCoreRegistry : CoreRegistry
    {
        public AndroidCoreRegistry(string repositoryPath, bool isEmbeded) : base(repositoryPath, isEmbeded)
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
                base.GetAssweblysForRegister().Concat(
                    new[] { typeof(ClientEventSync).Assembly, GetType().Assembly });
        }
        public override void Load()
        {
            this.Bind<IEventStore>().To<SQLiteEventStore>();
            base.Load();
        }
    }
}
