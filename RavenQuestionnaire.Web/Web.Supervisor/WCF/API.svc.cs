// --------------------------------------------------------------------------------------------------------------------
// <copyright file="API.svc.cs" company="">
//   
// </copyright>
// <summary>
//   The api.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Web.Supervisor.WCF
{
    using System;
    using System.IO;
    using System.ServiceModel.Activation;
    using System.Web;

    using DataEntryClient.SycProcess.Interfaces;
    using DataEntryClient.SycProcessFactory;

    using SynchronizationMessages.CompleteQuestionnaire;

    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "API" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select API.svc or API.svc.cs at the Solution Explorer and start debugging.

    /// <summary>
    /// The api.
    /// </summary>
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class API : IAPI
    {
        #region Fields

        /// <summary>
        /// The sync process factory.
        /// </summary>
        private readonly ISyncProcessFactory syncProcessFactory;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="API"/> class.
        /// </summary>
        public API()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="API"/> class. 
        /// Initializes a new instance of the <see cref="GetAggragateRootListService"/> class.
        /// </summary>
        /// <param name="syncProcessFactory">
        /// The sync Process Factory.
        /// </param>
        public API(ISyncProcessFactory syncProcessFactory)
        {
            this.syncProcessFactory = syncProcessFactory;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The get item.
        /// </summary>
        /// <param name="firstEventPulicKey">
        /// The first event pulic key.
        /// </param>
        /// <param name="length">
        /// The length.
        /// </param>
        /// <returns>
        /// The <see cref="Stream"/>.
        /// </returns>
        public Stream GetItem(string firstEventPulicKey, string length)
        {
            Guid syncProcess = Guid.NewGuid();

            Guid key;
            if (!Guid.TryParse(firstEventPulicKey, out key))
            {
                return null;
            }

            int ln;
            if (!int.TryParse(length, out ln))
            {
                return null;
            }

            var result = new ImportSynchronizationMessage();

            try
            {
                var process =
                    (IEventSyncProcess)this.syncProcessFactory.GetProcess(SyncProcessType.Event, syncProcess, null);

                result = process.Export("Supervisor export AR events", key, ln);
            }
            catch (Exception ex)
            {
            }

            HttpContext.Current.Response.ContentType = "application/json; charset=utf-8";
            var stream = new MemoryStream();
            result.WriteTo(stream);
            stream.Position = 0L;

            return stream;
        }

        /// <summary>
        /// The get roots list.
        /// </summary>
        /// <returns>
        /// The <see cref="Stream"/>.
        /// </returns>
        public Stream GetRootsList()
        {
            Guid syncProcess = Guid.NewGuid();

            var result = new ListOfAggregateRootsForImportMessage();

            try
            {
                var process =
                    (IEventSyncProcess)this.syncProcessFactory.GetProcess(SyncProcessType.Event, syncProcess, null);

                result = process.Export("Supervisor export AR events");
            }
            catch (Exception ex)
            {
            }

            HttpContext.Current.Response.ContentType = "application/json; charset=utf-8";
            var stream = new MemoryStream();
            result.WriteTo(stream);
            stream.Position = 0L;

            return stream;
        }

        /// <summary>
        /// The post stream.
        /// </summary>
        /// <param name="request">
        /// The request.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool PostStream(EventSyncMessage request)
        {
            Guid syncProcess = Guid.NewGuid();
            try
            {
                var process =
                    (IEventSyncProcess)
                    this.syncProcessFactory.GetProcess(SyncProcessType.Event, syncProcess, request.SynchronizationKey);

                process.Import("WCF syncronization", request.Command);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// The get roots list 1.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string test()
        {
            return "Test";
        }

        /// <summary>
        /// The test 1.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string test1()
        {
            return "OOK";
        }

        /// <summary>
        /// The test 2.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string test2(string id)
        {
            return "OK";
        }

        #endregion
    }
}