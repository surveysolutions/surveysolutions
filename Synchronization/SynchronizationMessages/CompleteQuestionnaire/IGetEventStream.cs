namespace SynchronizationMessages.CompleteQuestionnaire
{
    using System;
    using System.ServiceModel;

    /// <summary>
    /// The GetEventStream interface.
    /// </summary>
    [JsonNewSerializerContractBehavior]
    [ServiceContract]
    public interface IGetEventStream
    {
        #region Public Methods and Operators

        /// <summary>
        /// The process.
        /// </summary>
        /// <param name="firstEventPulicKey">
        /// The first event pulic key.
        /// </param>
        /// <param name="length">
        /// The length.
        /// </param>
        /// <returns>
        /// The SynchronizationMessages.CompleteQuestionnaire.ImportSynchronizationMessage.
        /// </returns>
        [OperationContract(Action = "http://tempuri.org/IGetEventStream/Process", 
            ReplyAction = "http://tempuri.org/IGetEventStream/ProcessResponse")]
        ImportSynchronizationMessage Process(Guid firstEventPulicKey, int length);

        #endregion
    }
}