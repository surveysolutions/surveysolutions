using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;

namespace DataEntryClient.WcfInfrastructure
{
    public interface IChanelFactoryWrapper
    {
        void Execute<T>(Action<T> handler) where T:class ;
        T GetChanel<T>() where T : class;
    }

    public class ChanelFactoryWrapper : IChanelFactoryWrapper
    {
        public void Execute<T>(Action<T> handler) where T : class
        {
            T client = GetChanel<T>();
            try
            {

                handler(client);
            }
            finally
            {
                try
                {
                    ((IChannel) client).Close();
                }
                catch
                {
                    ((IChannel) client).Abort();
                }
            }
        }

        public T GetChanel<T>() where T : class
        {
            ChannelFactory<T> channelFactory = new ChannelFactory<T>("");
            return channelFactory.CreateChannel();
        }
    }
}
