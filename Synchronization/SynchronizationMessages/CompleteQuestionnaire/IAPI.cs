namespace SynchronizationMessages.CompleteQuestionnaire
{
    using System.IO;
    using System.ServiceModel;
    using System.ServiceModel.Web;

    /// <summary>
    /// The API interface.
    /// </summary>
    [ServiceContract]
    public interface IAPI
    {
        #region Public Methods and Operators

        /// <summary>
        /// The get roots list.
        /// </summary>
        /// <returns>
        /// The <see cref="Stream"/>.
        /// </returns>
        [OperationContract]
        [WebInvoke(Method = "GET", UriTemplate = "/GetRootsList")]
        /*[WebInvoke(Method = "GET")]*/
        Stream GetRootsList();


        [WebInvoke(Method = "POST", UriTemplate = "/PostStream", 
            ResponseFormat = WebMessageFormat.Json)]
        bool PostStream(EventSyncMessage mess);


        /// <summary>
        /// The get roots list.
        /// </summary>
        /// <returns>
        /// The <see cref="Stream"/>.
        /// </returns>
        [OperationContract]
        [WebInvoke(Method = "GET", UriTemplate = "/GetItem/{firstEventPulicKey}/{length}")]
        /*[WebInvoke(Method = "GET")]*/
        Stream GetItem(string firstEventPulicKey, string length);


        /// <summary>
        /// The get roots list.
        /// </summary>
        /// <returns>
        /// The <see cref="Stream"/>.
        /// </returns>
        [OperationContract]
        /*[WebGet]*/
        [WebInvoke(Method = "GET", UriTemplate = "/test", ResponseFormat = WebMessageFormat.Json)]
        string test();


        
        [OperationContract]
        [WebInvoke(Method = "GET", UriTemplate = "/test2/{id}", ResponseFormat = WebMessageFormat.Json)]
        string test2(string id);


        /// <summary>
        /// The get roots list.
        /// </summary>
        /// <returns>
        /// The <see cref="Stream"/>.
        /// </returns>
        [OperationContract]
        /*[WebGet]*/
        [WebGet(UriTemplate = "/test1", ResponseFormat = WebMessageFormat.Json)]
        string test1();

        #endregion
    }
}