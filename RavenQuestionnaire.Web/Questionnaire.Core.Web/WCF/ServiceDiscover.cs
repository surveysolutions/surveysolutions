// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ServiceDiscover.cs" company="">
//   
// </copyright>
// <summary>
//   The service discover.
// </summary>
// --------------------------------------------------------------------------------------------------------------------


using WB.Core.SharedKernel.Utils.Logging;

namespace Questionnaire.Core.Web.WCF
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Discovery;


    using SynchronizationMessages.Discover;

    /// <summary>
    /// The service discover.
    /// </summary>
    public class ServiceDiscover
    {
        #region Fields

    
        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The discover channels.
        /// </summary>
        /// <returns>
        /// The System.Collections.Generic.IEnumerable`1[T -&gt; Questionnaire.Core.Web.WCF.ServiceDiscover+SyncSpot].
        /// </returns>
        public IEnumerable<SyncSpot> DiscoverChannels()
        {
            var dc = new DiscoveryClient(new UdpDiscoveryEndpoint());
            var fc = new FindCriteria(typeof(ISpotSync));
            fc.Duration = TimeSpan.FromSeconds(5);
            FindResponse fr = dc.Find(fc);

            // here is the really nasty part
            // i am just returning the first channel, but it may not work.
            // you have to do some logic to decide which uri to use from the discovered uris
            // for example, you may discover "127.0.0.1", but that one is obviously useless.
            // also, catch exceptions when no endpoints are found and try again.
            Collection<EndpointDiscoveryMetadata> endpoints = fr.Endpoints;
            var result = new List<SyncSpot>(endpoints.Count);
            foreach (EndpointDiscoveryMetadata endpointDiscoveryMetadata in endpoints)
            {
                var channelFactory = new ChannelFactory<ISpotSync>(
                    new BasicHttpBinding(), endpointDiscoveryMetadata.Address);
                ISpotSync client = channelFactory.CreateChannel();

                // T client = GetChanel<T>();
                try
                {
                    /*   Uri uri = endpointDiscoveryMetadata.Address.Uri;
                       var hostUri =new Uri( uri.Scheme + Uri.SchemeDelimiter + uri.Host + ":" + uri.Port);*/
                    string url = client.Process();
                    if (string.IsNullOrEmpty(url))
                    {
                        continue;
                    }

                    var hostUri = new Uri(url);
                    result.Add(new SyncSpot { SpotName = hostUri.Host, SpotUri = hostUri });

                    // handler(client);
                }
                catch (Exception e)
                {
                    LogManager.GetLogger(this.GetType()).Fatal(e);
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

            return result;
        }

        #endregion

        /// <summary>
        /// The sync spot.
        /// </summary>
        public class SyncSpot
        {
            #region Public Properties

            /// <summary>
            /// Gets or sets the spot name.
            /// </summary>
            public string SpotName { get; set; }

            /// <summary>
            /// Gets or sets the spot uri.
            /// </summary>
            public Uri SpotUri { get; set; }

            #endregion
        }
    }
}