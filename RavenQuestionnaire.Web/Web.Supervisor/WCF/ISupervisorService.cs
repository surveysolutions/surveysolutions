// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISupervisorService.cs" company="">
//   
// </copyright>
// <summary>
//   The i authorization request.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Web.Supervisor.WCF
{
    using System.Collections.Generic;
    using System.ServiceModel;

    using Main.Core.Entities;

    /// <summary>
    /// The i authorization request.
    /// </summary>
    public interface IAuthorizationRequest
    {
        #region Public Properties

        /// <summary>
        /// Gets Data.
        /// </summary>
        RegisterData Data { get; }

        /// <summary>
        /// Gets a value indicating whether IsAuthorized.
        /// </summary>
        bool IsAuthorized { get; }

        #endregion
    }

    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "ISupervisorService" in both code and config file together.
    
    /// <summary>
    /// The i supervisor service.
    /// </summary>
    [ServiceContract]
    public interface ISupervisorService
    {
        #region Public Methods and Operators

        /// <summary>
        /// The authorize device.
        /// </summary>
        /// <param name="registerData">
        /// The register data.
        /// </param>
        /// <returns>
        /// The authorize device
        /// </returns>
        [OperationContract]
        bool AuthorizeDevice(byte[] registerData);

        /// <summary>
        /// The get authorization requests.
        /// </summary>
        /// <returns>
        /// List of authorization requests
        /// </returns>
        [OperationContract]
        IList<IAuthorizationRequest> GetAuthorizationRequests();

        /// <summary>
        /// The get path.
        /// </summary>
        /// <returns>
        /// The get path
        /// </returns>
        [OperationContract]
        string GetPath();

        #endregion
    }
}