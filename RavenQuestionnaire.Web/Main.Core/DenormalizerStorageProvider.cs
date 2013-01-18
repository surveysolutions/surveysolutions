﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DenormalizerStorageProvider.cs" company="The World Bank">
//   The World Bank
// </copyright>
// <summary>
//   TODO: Update summary.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Core
{
    using System;

    using Main.DenormalizerStorage;

    using Ninject;
    using Ninject.Activation;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    public class DenormalizerStorageProvider<T> : Provider<IDenormalizerStorage<T>>
        where T : class
    {
        #region Methods

        /// <summary>
        /// The create instance.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <returns>
        /// The <see cref="IDenormalizerStorage"/>.
        /// </returns>
        protected override IDenormalizerStorage<T> CreateInstance(IContext context)
        {
            IDenormalizerStorage<T> result;
            try
            {
                if (this.GetWeekBinding())
                {
                    result = context.Kernel.Get<PersistentDenormalizer<T>>();
                }
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

        /// <summary>
        /// The get week binding.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        protected bool GetWeekBinding()
        {
            return typeof(T).GetCustomAttributes(typeof(SmartDenormalizerAttribute), true).Length > 0;
        }

        #endregion
    }
}