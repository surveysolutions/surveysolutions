using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Discovery;
using System.Text;
using SynchronizationMessages.Discover;

namespace Questionnaire.Core.Web.WCF
{
    public class ServiceDiscover
    {
        public IEnumerable<SyncSpot> DiscoverChannels()
        {
            var dc = new DiscoveryClient(new UdpDiscoveryEndpoint());
            FindCriteria fc = new FindCriteria(typeof(ISpotSync));
            fc.Duration = TimeSpan.FromSeconds(5);
            FindResponse fr = dc.Find(fc);

            // here is the really nasty part
            // i am just returning the first channel, but it may not work.
            // you have to do some logic to decide which uri to use from the discovered uris
            // for example, you may discover "127.0.0.1", but that one is obviously useless.
            // also, catch exceptions when no endpoints are found and try again.
            var endpoints = fr.Endpoints;
            var result = new List<SyncSpot>(endpoints.Count);
            foreach (EndpointDiscoveryMetadata endpointDiscoveryMetadata in endpoints)
            {
                ChannelFactory<ISpotSync> channelFactory = new ChannelFactory<ISpotSync>(new BasicHttpBinding(),endpointDiscoveryMetadata.Address);
                var client = channelFactory.CreateChannel();
             //   T client = GetChanel<T>();
                try
                {
                    /*   Uri uri = endpointDiscoveryMetadata.Address.Uri;
                       var hostUri =new Uri( uri.Scheme + Uri.SchemeDelimiter + uri.Host + ":" + uri.Port);*/
                    var url = client.Process();
                    if (string.IsNullOrEmpty(url))
                        continue;
                    var hostUri = new Uri(url);
                    result.Add(new SyncSpot() { SpotName = hostUri.Host, SpotUri = hostUri });
                    //  handler(client);
                }
                catch (Exception e)
                {
                    NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
                    logger.Fatal(e);
                
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
            return result;
        }
        public class SyncSpot
        {
            public string SpotName { get; set; }
            public Uri SpotUri { get; set; }
        }
    }

}
