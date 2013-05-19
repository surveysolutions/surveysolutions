// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RemoteCollector.cs" company="">
//   
// </copyright>
// <summary>
//   The remote collector.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AndroidMain.Synchronization
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;

    using Main.Core.Events;
    using Main.Core.View.SyncProcess;
    using Main.Synchronization.SyncStreamCollector;

    using Newtonsoft.Json;

    using RestSharp;

    using SynchronizationMessages.CompleteQuestionnaire;
    using SynchronizationMessages.Synchronization;

    /// <summary>
    /// The remote collector.
    /// </summary>
    public class RemoteCollector : ISyncStreamCollector
    {

        /// <summary>
        /// The item path.
        /// </summary>
        private const string pushPath = "importexport/PostStream";

        /// <summary>
        /// The item path 1.
        /// </summary>
        private const string pushPath1 = "importexport/PostStream1";


        private const string GetCurrentVersionPath = "importexport/GetCurrentVersion";

        /// <summary>
        /// The base address.
        /// </summary>
        private readonly string baseAddress;

        /// <summary>
        /// Gets or sets the process guid.
        /// </summary>
        /// <exception cref="NotImplementedException">
        /// </exception>
        protected Guid ProcessGuid { get; private set; }

        #region Public Properties

        /// <summary>
        /// Gets the max chunk size.
        /// </summary>
        public int MaxChunkSize { get; private set; }

        /// <summary>
        /// Gets a value indicating whether support sync stat.
        /// </summary>
        public bool SupportSyncStat
        {
            get
            {
                return false;
            }
        }


        public RemoteCollector(string baseAddress, Guid processGuid)
        {
            this.baseAddress = baseAddress;
            this.ProcessGuid = processGuid;
            this.MaxChunkSize = 100;
        }


        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The collect.
        /// </summary>
        /// <param name="chunk">
        /// The chunk.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public bool Collect(IEnumerable<AggregateRootEvent> chunk)
        {
            var restClient = new RestClient(this.baseAddress);

            var request = new RestRequest(pushPath, Method.POST);
            request.RequestFormat = DataFormat.Json;
            request.AddHeader("Accept-Encoding", "gzip,deflate");

            try
            {
                var message = new EventSyncMessage { Command = chunk.ToArray(), SynchronizationKey = this.ProcessGuid };
                
                var settings = new JsonSerializerSettings();
                settings.TypeNameHandling = TypeNameHandling.Objects;
                string item = JsonConvert.SerializeObject(message, Formatting.None, settings);

                // text must be changed to  "application/json; charset=utf-8"
                request.AddParameter("text; charset=utf-8", item, ParameterType.RequestBody);
                
                IRestResponse response = restClient.Execute(request);
                if (string.IsNullOrWhiteSpace(response.Content) || response.StatusCode != HttpStatusCode.OK)
                {
                    return false;
                }

                return true;
            }
            catch (Exception exc)
            {
                // log
                throw;
                //return false;
            }
            
        }

        /// <summary>
        /// The finish.
        /// </summary>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public void Finish()
        {
            
        }

        /// <summary>
        /// The get stat.
        /// </summary>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public List<UserSyncProcessStatistics> GetStat()
        {
            return null;
        }

        /// <summary>
        /// The prepare to collect.
        /// </summary>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public void PrepareToCollect()
        {
            
        }

        #endregion
    }
}