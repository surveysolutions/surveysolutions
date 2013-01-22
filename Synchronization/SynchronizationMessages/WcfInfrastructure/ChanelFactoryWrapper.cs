// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChanelFactoryWrapper.cs" company="The World Bank">
//   The World Bank
// </copyright>
// <summary>
//   The chanel factory wrapper.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SynchronizationMessages.WcfInfrastructure
{
    using System;
    using System.Linq;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

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
        /// The base address.
        /// </param>
        /// <param name="handler">
        /// The handler.
        /// </param>
        /// <typeparam name="T">
        /// Classes only
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
        /// The base address.
        /// </param>
        /// <typeparam name="T">
        /// Classes only
        /// </typeparam>
        /// <returns>
        /// The T.
        /// </returns>
        public T GetChanel<T>(string baseAdress) where T : class
        {
            if (baseAdress.Last() == '/')
            {
                baseAdress = baseAdress.TrimEnd(new[] { '/' });
            }

            var channelFactory = new ChannelFactory<T>(
                new BasicHttpBinding("basicBindingDiscovery"), 
                string.Format("{0}/WCF/{1}Service.svc", baseAdress, typeof(T).Name.Substring(1)));
            return channelFactory.CreateChannel();
        }

        #endregion
    }
}