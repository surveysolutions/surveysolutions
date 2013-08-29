namespace SynchronizationMessages.WcfInfrastructure
{
    using System;

    /// <summary>
    /// The ChanelFactoryWrapper interface.
    /// </summary>
    public interface IChanelFactoryWrapper
    {
        #region Public Methods and Operators

        /// <summary>
        /// The execute.
        /// </summary>
        /// <param name="baseAdress">
        /// The base address.
        /// </param>
        /// <param name="handler">
        /// The handler.
        /// </param>
        /// <typeparam name="T">
        /// Classes only
        /// </typeparam>
        void Execute<T>(string baseAdress, Action<T> handler) where T : class;

        /// <summary>
        /// The get chanel.
        /// </summary>
        /// <param name="baseAdress">
        /// The base address.
        /// </param>
        /// <typeparam name="T">
        /// Classes only
        /// </typeparam>
        /// <returns>
        /// The T.
        /// </returns>
        T GetChanel<T>(string baseAdress) where T : class;

        #endregion
    }
}