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
    using SynchronizationMessages.WcfInfrastructure;

    /// <summary>
    /// The remote service event stream provider.
    /// </summary>
    public class RemoteServiceEventStreamProvider1 : IIntSyncEventStreamProvider
    {
        #region Constants

        /// <summary>
        /// The item path.
        /// </summary>
        private const string itemPath = "importexport/GetItem";

        /// <summary>
        /// The item path 1.
        /// </summary>
        private const string itemPath1 = "importexport/GetItem1";

        /// <summary>
        /// The list path.
        /// </summary>
        private const string listPath = "importexport/GetRootsList";

        /// <summary>
        /// The list path 1.
        /// </summary>
        private const string listPath1 = "importexport/GetRootsList1";

        #endregion

        #region Fields

        /// <summary>
        /// The base address.
        /// </summary>
        private readonly string baseAddress;

        /// <summary>
        /// The chanel factory wrapper.
        /// </summary>
        private readonly IChanelFactoryWrapper chanelFactoryWrapper;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteServiceEventStreamProvider1"/> class.
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
        public RemoteServiceEventStreamProvider1(IKernel kernel, Guid processGuid, string address)
        {
            this.chanelFactoryWrapper = kernel.Get<IChanelFactoryWrapper>();
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
            
            var request = new RestRequest(listPath, Method.GET);
            request.RequestFormat = DataFormat.Json;

            IRestResponse response = restClient.Execute(request);
            if (string.IsNullOrWhiteSpace(response.Content) || response.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            var listOfAggregateRootsForImportMessage =
                JsonConvert.DeserializeObject<ListOfAggregateRootsForImportMessage>(
                    response.Content, new JsonSerializerSettings());

            /*var roots = JsonConvert.DeserializeObject<IList<ProcessedEventChunk>>(
                response.Content.Substring(pos), new JsonSerializerSettings());*/
            if (listOfAggregateRootsForImportMessage == null)
            {
                throw new Exception("aggregate roots list is empty");
            }

            var events = new List<AggregateRootEvent>();

            foreach (ProcessedEventChunk root in listOfAggregateRootsForImportMessage.Roots)
            {
                try
                {
                    if (root.EventKeys.Count == 0)
                    {
                        continue;
                    }

                    var itemRequest = new RestRequest(itemPath1, Method.POST);
                    itemRequest.AddParameter("firstEventPulicKey", root.EventKeys.First());
                    itemRequest.AddParameter("length", root.EventKeys.Count);

                    itemRequest.RequestFormat = DataFormat.Json;

                    
                    IRestResponse responseStream = restClient.Execute(itemRequest);
                    if (string.IsNullOrWhiteSpace(responseStream.Content)
                        || responseStream.StatusCode != HttpStatusCode.OK)
                    {
                        throw new Exception("Error stream for item " + root.EventKeys.First());
                    }
                    
                    //ImportSynchronizationMessage item = new ImportSynchronizationMessage();

                    var evnts = JsonConvert.DeserializeObject<AggregateRootEvent[]>(responseStream.Content.Substring(responseStream.Content.IndexOf("[")), new JsonSerializerSettings());

                    //var streamMessage = JsonConvert.DeserializeObject<ImportSynchronizationMessage>(response.Content);
                    if (evnts != null)
                    {
                        events.AddRange(evnts);
                    }

                    /*this.Invoker.Execute(
                                new ChangeEventStatusCommand(
                                    this.ProcessGuid, root.EventChunckPublicKey, EventState.Completed));*/
                }
                catch (Exception ex)
                {
                    /*Logger logger = LogManager.GetCurrentClassLogger();
                            logger.Fatal("Import error", ex);*/
                    events = null;

                    /* this.Invoker.Execute(
                                new ChangeEventStatusCommand(
                                    this.ProcessGuid, root.EventChunckPublicKey, EventState.Error));*/
                }
            }

            return events;
        }

        #endregion
    }
}