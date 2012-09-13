// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IChanelFactoryWrapper.cs" company="">
//   
// </copyright>
// <summary>
//   The ChanelFactoryWrapper interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataEntryClient.WcfInfrastructure
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

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
        /// The base adress.
        /// </param>
        /// <param name="handler">
        /// The handler.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        void Execute<T>(string baseAdress, Action<T> handler) where T : class;

        /// <summary>
        /// The get chanel.
        /// </summary>
        /// <param name="baseAdress">
        /// The base adress.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The T.
        /// </returns>
        T GetChanel<T>(string baseAdress) where T : class;

        #endregion
    }

    /// <summary>
    /// The chanel factory wrapper.
    /// </summary>
    public class ChanelFactoryWrapper : IChanelFactoryWrapper
    {
        #region Public Methods and Operators

        /// <summary>
        /// The execute.
        /// </summary>
        /// <param name="baseAdress">
        /// The base adress.
        /// </param>
        /// <param name="handler">
        /// The handler.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        public void Execute<T>(string baseAdress, Action<T> handler) where T : class
        {
            var client = this.GetChanel<T>(baseAdress);
            try
            {
                handler(client);
            }
            finally
            {
                try
                {
                    ((IChannel)client).Close();
                }
                catch
                {
                    ((IChannel)client).Abort();
                }
            }
        }

        /// <summary>
        /// The get chanel.
        /// </summary>
        /// <param name="baseAdress">
        /// The base adress.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The T.
        /// </returns>
        public T GetChanel<T>(string baseAdress) where T : class
        {
            var channelFactory = new ChannelFactory<T>(
                new BasicHttpBinding("basicBindingDiscovery"), 
                string.Format("{0}/WCF/{1}Service.svc", baseAdress, typeof(T).Name.Substring(1)));
            return channelFactory.CreateChannel();
        }

        #endregion
    }
}