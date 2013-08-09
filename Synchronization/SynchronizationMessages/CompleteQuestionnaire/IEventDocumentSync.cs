namespace SynchronizationMessages.CompleteQuestionnaire
{
    using System.ServiceModel;

    /// <summary>
    /// This is an example of using a service contract interface
    /// instead of a service reference. Make sure to include the
    /// Action / ReplyAction values that correspond to your
    /// service inputs and outputs. A simple way to find out these
    /// values is to host the service and inspect the auto-generated
    /// WSDL by appending ?wsdl to the URL of the service.
    /// </summary>
    [JsonNewSerializerContractBehavior]
    [ServiceContract]
    public interface IEventPipe
    {
        #region Public Methods and Operators

        /// <summary>
        /// The process.
        /// </summary>
        /// <param name="request">
        /// The request.
        /// </param>
        /// <returns>
        /// The SynchronizationMessages.CompleteQuestionnaire.ErrorCodes.
        /// </returns>
        [OperationContract(Action = "http://tempuri.org/IEventPipe/Process", 
            ReplyAction = "http://tempuri.org/IEventPipe/ProcessResponse")]
        ErrorCodes Process(EventSyncMessage request);

        #endregion
    }
}