// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Provider.cs" company="">
//   
// </copyright>
// <summary>
//   The remote service event stream provider.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Security.Authentication;
using Main.Core.Entities.SubEntities;
using Main.Synchronization.Credentials;
using Ncqrs;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;

namespace AndroidMain.Synchronization
{
    using System;
    using System.Collections.Generic;
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

        private bool UseGZip = true;

        private readonly ISyncAuthenticator validator;

        #endregion

        #region Constructors and Destructors

        public RemoteServiceEventStreamRestProvider(Guid processGuid, string address, ISyncAuthenticator validator)
        {
            this.ProcessGuid = processGuid;
            this.baseAddress = address;

            this.validator = validator;
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

            if (UseGZip)
                request.AddHeader("Accept-Encoding", "gzip,deflate");

            var currentCredentials = validator.RequestCredentials();
            request.AddParameter("login", currentCredentials.Login);
            request.AddParameter("password", currentCredentials.Password);

            IRestResponse response = restClient.Execute(request);
            
            if (string.IsNullOrWhiteSpace(response.Content) || response.StatusCode != HttpStatusCode.OK)
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                    throw new AuthenticationException("user wasn't authorized");
                throw new Exception("Target returned unsupported result.");
            }

            var syncItemsMetaContainer =
                JsonConvert.DeserializeObject<SyncItemsMetaContainer>(
                    response.Content, new JsonSerializerSettings());

            /*var roots = JsonConvert.DeserializeObject<IList<ProcessedEventChunk>>(
                response.Content.Substring(pos), new JsonSerializerSettings());*/
            if (syncItemsMetaContainer == null)
            {
                throw new Exception("Elements to be synchronized are not found.");
            }

            //var events = new List<AggregateRootEvent>();

            foreach (var root in syncItemsMetaContainer.ARId)
            {
                if (SkipAgregateRootIfNoChanges(root))
                    continue;
                var itemRequest = new RestRequest(GetARPath, Method.POST);
                itemRequest.AddParameter("ARKey", root.AggregateRootId);
                itemRequest.AddParameter("length", 0);
                itemRequest.AddParameter("rootType", root.AggregateRootType);
                itemRequest.AddParameter("login", currentCredentials.Login);
                itemRequest.AddParameter("password", currentCredentials.Password);
                itemRequest.RequestFormat = DataFormat.Json;

                if (UseGZip)
                    itemRequest.AddHeader("Accept-Encoding", "gzip,deflate");

                IRestResponse responseStream = restClient.Execute(itemRequest);
                if (string.IsNullOrWhiteSpace(responseStream.Content) || responseStream.StatusCode != HttpStatusCode.OK)
                {
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                        throw new AuthenticationException("user wasn't authorized");
                    //logging
                    throw new Exception("Operation finished unsuccessfully. Item was not received.");
                }

                var settings = new JsonSerializerSettings() {TypeNameHandling = TypeNameHandling.Objects};
                var str = responseStream.Content.Substring(responseStream.Content.IndexOf("["));
                var evnts = JsonConvert.DeserializeObject<AggregateRootEvent[]>(str, settings);


                foreach (var aggregateRootEvent in evnts)
                {
                    yield return aggregateRootEvent;
                }
            }

            //return events;
        }

        private static bool SkipAgregateRootIfNoChanges(SyncItemsMeta root)
        {
            if (!root.AggregateRootPeak.HasValue)
                return false;
            var eventStore = NcqrsEnvironment.Get<IEventStore>();
            if (eventStore == null)
                return false;
            var streamableEventStore = eventStore as IStreamableEventStore;
            if (streamableEventStore == null)
                return false;
            return streamableEventStore.IsEventPresent(root.AggregateRootId, root.AggregateRootPeak.Value);
        }

        #endregion
    }
}