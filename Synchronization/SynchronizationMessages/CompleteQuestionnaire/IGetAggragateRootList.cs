// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IGetAggragateRootList.cs" company="">
//   
// </copyright>
// <summary>
//   The GetAggragateRootList interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SynchronizationMessages.CompleteQuestionnaire
{
    using System.IO;
    using System.ServiceModel;

    /// <summary>
    /// The GetAggragateRootList interface.
    /// </summary>
    /*[JsonNewSerializerContractBehavior]*/
    [ServiceContract]
    public interface IGetAggragateRootList
    {
        #region Public Methods and Operators

        /// <summary>
        /// The process.
        /// </summary>
        /// <returns>
        /// The SynchronizationMessages.CompleteQuestionnaire.ListOfAggregateRootsForImportMessage.
        /// </returns>
        [OperationContract(Action = "http://tempuri.org/IGetAggragateRootList/Process", 
            ReplyAction = "http://tempuri.org/IGetAggragateRootList/ProcessResponse")]
        ListOfAggregateRootsForImportMessage Process();

        #endregion
    }
}