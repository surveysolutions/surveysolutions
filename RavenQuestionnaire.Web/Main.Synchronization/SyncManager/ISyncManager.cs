﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISyncManager.cs" company="">
//   
// </copyright>
// <summary>
//   The SyncManager interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Synchronization.SyncManager
{
    /// <summary>
    /// The SyncManager interface.
    /// </summary>
    public interface ISyncManager
    {
        #region Public Properties

        /// <summary>
        /// Gets a value indicating whether is working.
        /// </summary>
        bool IsWorking { get; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The pull.
        /// </summary>
        void StartPull();

        /// <summary>
        /// The push.
        /// </summary>
        void StartPush();

        /// <summary>
        /// The start sync.
        /// </summary>
        void StartSynchronization();

        /// <summary>
        /// The stop sync.
        /// </summary>
        void StopProcess();

        /// <summary>
        /// The get current progress.
        /// </summary>
        /// <returns>
        /// The <see cref="int?"/>.
        /// </returns>
        int? GetCurrentProgress();

        #endregion
    }
}