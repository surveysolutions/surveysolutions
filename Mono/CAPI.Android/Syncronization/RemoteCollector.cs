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

namespace CAPI.Android.Syncronization
{
    using SynchronizationMessages.Synchronization;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using Main.Core.Events;
    using Main.Core.View.SyncProcess;
    using Main.Synchronization.SyncStreamCollector;
    using Newtonsoft.Json;
    using RestSharp;
    using SynchronizationMessages.CompleteQuestionnaire;


    /// <summary>
    /// The remote collector.
    /// </summary>
    public class RemoteCollector : ISyncStreamCollector
    {
        private bool useCompression = true;

        /// <summary>
        /// The item path.
        /// </summary>
        private const string pushPath = "importexport/PostStream";

        private const string GetCurrentVersionPath = "importexport/GetCurrentVersion";
        private const string fileName = "eventstream";

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
            //restClient. 
          
            var restClient = new RestClient(this.baseAddress);
            var currentCredentials = validator.RequestCredentials();
            var request = new RestRequest(pushPath/*string.Format("{0}?login={1}&password={2}", pushPath, currentCredentials.Login, currentCredentials.Password)*/, Method.POST);
            request.IncreaseNumAttempts();
            request.IncreaseNumAttempts();

            request.RequestFormat = DataFormat.Json;
            request.AddHeader("Accept-Encoding", "gzip,deflate");

            request.AddParameter("login", currentCredentials.Login, ParameterType.GetOrPost);
            request.AddParameter("password", currentCredentials.Password, ParameterType.GetOrPost);
            
            try
            {
                var message = new EventSyncMessage { Command = chunk.ToArray(), SynchronizationKey = this.ProcessGuid };
                
                var settings = new JsonSerializerSettings();
                settings.TypeNameHandling = TypeNameHandling.Objects;
                string itemToSync = JsonConvert.SerializeObject(message, Formatting.None, settings);

                if (useCompression)
                {
                    request.AddFile(fileName, PackageHelper.Compress(itemToSync), fileName);
                }
                else
                {
                    request.AddFile(fileName, GetBytes(itemToSync), fileName);
                }
             
                IRestResponse response = restClient.Execute(request);
                if (string.IsNullOrWhiteSpace(response.Content) || response.StatusCode != HttpStatusCode.OK)
                {
                    if (response.StatusCode == HttpStatusCode.Forbidden)
                        throw new AuthenticationException("user wasn't authorized");
                    return false;
                }

                return IsOperationSucceded(response.Content);
            }
            catch (Exception exc)
            {
                // log
                throw;
                //return false;
            }   
        }
        private byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes; 
        }

        private bool IsOperationSucceded(string response)
        {
            return string.CompareOrdinal(response, "True") == 0;
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