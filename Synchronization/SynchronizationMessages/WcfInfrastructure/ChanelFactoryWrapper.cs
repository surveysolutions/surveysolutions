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

            var binding = new BasicHttpBinding(BasicHttpSecurityMode.None);
            binding.MaxReceivedMessageSize = 2147483647;
            binding.MaxBufferSize = 2147483647;

            binding.ReaderQuotas.MaxArrayLength = 2147483647;
            binding.ReaderQuotas.MaxBytesPerRead = 2147483647;
            binding.ReaderQuotas.MaxNameTableCharCount = 2147483647;
            binding.ReaderQuotas.MaxStringContentLength = 2147483647;


            var channelFactory = new ChannelFactory<T>(
                binding,
                string.Format("{0}/WCF/{1}Service.svc", baseAdress, typeof(T).Name.Substring(1)));
            return channelFactory.CreateChannel();
        }

        #endregion
    }
}