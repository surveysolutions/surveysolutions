// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ServiceDiscover.cs" company="">
//   
// </copyright>
// <summary>
//   The service discover.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataEntryClient.WcfInfrastructure
{
    using System;
    using System.ServiceModel.Discovery;

    using SynchronizationMessages.Synchronization;

    /// <summary>
    /// The service discover.
    /// </summary>
    public class ServiceDiscover
    {
        #region Public Methods and Operators

        /// <summary>
        /// The wcf test client_ discover channel.
        /// </summary>
        /// <returns>
        /// The System.Uri.
        /// </returns>
        public Uri WcfTestClient_DiscoverChannel()
        {
            var dc = new DiscoveryClient(new UdpDiscoveryEndpoint());
            var fc = new FindCriteria(typeof(IGetLastSyncEvent));
            fc.Duration = TimeSpan.FromSeconds(5);
            FindResponse fr = dc.Find(fc);
            foreach (EndpointDiscoveryMetadata edm in fr.Endpoints)
            {
                Console.WriteLine("uri found = " + edm.Address.Uri);
            }

            // here is the really nasty part
            // i am just returning the first channel, but it may not work.
            // you have to do some logic to decide which uri to use from the discovered uris
            // for example, you may discover "127.0.0.1", but that one is obviously useless.
            // also, catch exceptions when no endpoints are found and try again.
            return fr.Endpoints[0].Address.Uri;
        }

        #endregion

        /*  public void WcfTestClient_SetupChannel()
        {
            var binding = new BasicHttpBinding(BasicHttpSecurityMode.None);
            var factory = new ChannelFactory<IWcfPingTest>(binding);
            var uri = WcfTestClient_DiscoverChannel();
            Console.WriteLine("creating channel to " + uri.ToString());
            EndpointAddress ea = new EndpointAddress(uri);
            channel = factory.CreateChannel(ea);
            Console.WriteLine("channel created");
            //Console.WriteLine("pinging host");
            //string result = channel.Ping();
            //Console.WriteLine("ping result = " + result);
        }
        public void WcfTestClient_Ping()
        {
            Console.WriteLine("pinging host");
            string result = channel.Ping();
            Console.WriteLine("ping result = " + result);
        }*/
    }
}