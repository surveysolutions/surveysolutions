namespace SynchronizationMessages.Synchronization
{
    using System;
    using System.ServiceModel;

    using SynchronizationMessages.CompleteQuestionnaire;

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
    public interface IGetLastSyncEvent
    {
        #region Public Methods and Operators

        /// <summary>
        /// The process.
        /// </summary>
        /// <param name="clientGuid">
        /// The client guid.
        /// </param>
        /// <returns>
        /// The System.Nullable`1[T -&gt; System.Guid].
        /// </returns>
        [OperationContract(Action = "http://tempuri.org/IGetLastSyncEvent/Process", 
            ReplyAction = "http://tempuri.org/IGetLastSyncEvent/ProcessResponse")]
        Guid? Process(Guid clientGuid);

        #endregion
    }
}