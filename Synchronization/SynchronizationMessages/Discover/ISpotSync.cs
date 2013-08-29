namespace SynchronizationMessages.Discover
{
    using System.ServiceModel;

    /// <summary>
    /// The SpotSync interface.
    /// </summary>
    [ServiceContract]
    public interface ISpotSync
    {
        #region Public Methods and Operators

        /// <summary>
        /// The process.
        /// </summary>
        /// <returns>
        /// The System.String.
        /// </returns>
        [OperationContract(Action = "http://tempuri.org/ISpotSync/Process", 
            ReplyAction = "http://tempuri.org/ISpotSync/ProcessResponse")]
        string Process();

        #endregion
    }
}