// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RemoteCollector.cs" company="">
//   
// </copyright>
// <summary>
//   The remote collector.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Security.Authentication;
using Main.Core.Entities.SubEntities;
using Main.Synchronization.Credentials;

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

        private readonly UserLight credentials;


        /// <summary>
        /// The base address.
        /// </summary>
        private readonly string baseAddress;

        private readonly ISyncAuthenticator validator;

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


        public RemoteCollector(string baseAddress, Guid processGuid, ISyncAuthenticator validator)
        {
            this.baseAddress = baseAddress;
            this.ProcessGuid = processGuid;
            this.validator = validator;
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
            if (!validator.ValidateUser())
            {
                throw new AuthenticationException("User wasn't authenticated");
            }

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