// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Provider.cs" company="">
//   
// </copyright>
// <summary>
//   The remote service event stream provider.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace AndroidMain.Synchronization
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;

    using Main.Core.Documents;
    using Main.Core.Events;
    using Main.Synchronization.SyncSreamProvider;

    using Newtonsoft.Json;

    using Ninject;

    using RestSharp;

    using SynchronizationMessages.CompleteQuestionnaire;

    /// <summary>
    /// The remote service event stream provider.
    /// </summary>
    public class RemoteServiceEventStreamRestProvider : IIntSyncEventStreamProvider
    {
        #region Constants

        /// <summary>
        /// The item path.
        /// </summary>
        private const string itemPath = "importexport/GetItem";

        /// <summary>
        /// The item path 1.
        /// </summary>
        private const string GetItemAsStreamPath = "importexport/GetItemAsStream";

        /// <summary>
        /// The list path.
        /// </summary>
        private const string GetRootsListPath = "importexport/GetRootsList";

        /// <summary>
        /// The list path.
        /// </summary>
        private const string GetARKeysPath = "importexport/GetARKeys";

        /// <summary>
        /// The list path.
        /// </summary>
        private const string GetARPath = "importexport/GetAR";

        #endregion

        #region Fields

        /// <summary>
        /// The base address.
        /// </summary>
        private readonly string baseAddress;

       /* /// <summary>
        /// The chanel factory wrapper.
        /// </summary>
        private readonly IChanelFactoryWrapper chanelFactoryWrapper;*/

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteServiceEventStreamRestProvider"/> class.
        /// </summary>
        /// <param name="kernel">
        /// The kernel.
        /// </param>
        /// <param name="processGuid">
        /// The process guid.
        /// </param>
        /// <param name="address">
        /// The address.
        /// </param>
        public RemoteServiceEventStreamRestProvider(IKernel kernel, Guid processGuid, string address)
        {
            this.ProcessGuid = processGuid;
            this.baseAddress = address;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the provider name.
        /// </summary>
        public string ProviderName
        {
            get
            {
                return "Remote Service Reader";
            }
        }

        /// <summary>
        /// Gets the sync type.
        /// </summary>
        public SynchronizationType SyncType
        {
            get
            {
                return SynchronizationType.Pull;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the process guid.
        /// </summary>
        /// <exception cref="NotImplementedException">
        /// </exception>
        protected Guid ProcessGuid { get; private set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The get event stream.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public IEnumerable<AggregateRootEvent> GetEventStream()
        {
            return this.GetEventStreamWithProxy();
        }

        /// <summary>
        /// The get total event count.
        /// </summary>
        /// <returns>
        /// The <see cref="int?"/>.
        /// </returns>
        public int? GetTotalEventCount()
        {
            return null;
        }

        #endregion

        #region Methods

        /// <summary>
        /// The get event stream with proxy.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable"/>.
        /// </returns>
        /// <exception cref="Exception">
        /// </exception>
        protected IEnumerable<AggregateRootEvent> GetEventStreamWithProxy()
        {
            var restClient = new RestClient(this.baseAddress);

            var request = new RestRequest(GetARKeysPath, Method.POST);
            request.RequestFormat = DataFormat.Json;
            request.AddHeader("Accept-Encoding", "gzip,deflate");

            IRestResponse response = restClient.Execute(request);


            if (string.IsNullOrWhiteSpace(response.Content) || response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception("Event list is empty.");
            }

            var syncItemsMetaContainer =
                JsonConvert.DeserializeObject<SyncItemsMetaContainer>(
                    response.Content, new JsonSerializerSettings());

            /*var roots = JsonConvert.DeserializeObject<IList<ProcessedEventChunk>>(
                response.Content.Substring(pos), new JsonSerializerSettings());*/
            if (syncItemsMetaContainer == null)
            {
                throw new Exception("aggregate roots list is empty");
            }

            //var events = new List<AggregateRootEvent>();

            foreach (var root in syncItemsMetaContainer.ARId)
            {

                var itemRequest = new RestRequest(GetARPath, Method.POST);
                    itemRequest.AddParameter("ARKey", root.Item2);
                    //itemRequest.AddParameter("length", root.EventKeys.Count);
                    itemRequest.AddParameter("rootType", root.Item1);


                    itemRequest.RequestFormat = DataFormat.Json;
                    itemRequest.AddHeader("Accept-Encoding", "gzip,deflate");
                    
                    IRestResponse responseStream = restClient.Execute(itemRequest);
                    if (string.IsNullOrWhiteSpace(responseStream.Content)
                        || responseStream.StatusCode != HttpStatusCode.OK)
                    {
                        throw new Exception("Error stream for item " + root.Item2);
                    }
                    
                    var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
                    var str = responseStream.Content.Substring( responseStream.Content.IndexOf("[") );
                    var evnts = JsonConvert.DeserializeObject<AggregateRootEvent[]>(str, settings);


                    foreach (var aggregateRootEvent in evnts)
                    {
                        yield return aggregateRootEvent;
                    }
            }

            //return events;
        }

        #endregion
    }
}