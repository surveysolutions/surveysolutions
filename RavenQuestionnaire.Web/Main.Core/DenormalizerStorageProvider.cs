// -----------------------------------------------------------------------
// <copyright file="DenormalizerStorageProvider.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using Main.DenormalizerStorage;
using Ninject;
using Ninject.Activation;

namespace Main.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class DenormalizerStorageProvider<T> : Provider<IDenormalizerStorage<T>> where T : class
    {
        #region Overrides of Provider<T>

        protected override IDenormalizerStorage<T> CreateInstance(IContext context)
        {
            IDenormalizerStorage<T> result;
            try
            {
                if (GetWeekBinding())
                    result = context.Kernel.Get<WeakReferenceDenormalizer<T>>();
                else
                {
                    result = context.Kernel.Get<InMemoryDenormalizer<T>>();
                }
            }
            catch (Exception ex)
            {
                throw; // new Exception(ex.Message, ex);
            }

            return result;
        }

        #endregion

        protected bool GetWeekBinding()
        {
            return typeof (T).GetCustomAttributes(typeof (SmartDenormalizerAttribute), true).Length > 0;
        }
    }
}
