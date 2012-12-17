// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SupervisorService.svc.cs" company="The World Bank">
//   Supervisor Service
// </copyright>
// <summary>
//   The supervisor service.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Web.Supervisor.WCF
{
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.Web.Hosting;

    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "SupervisorService" in code, svc and config file together.
    
    /// <summary>
    /// The supervisor service.
    /// </summary>
    public class SupervisorService : ISupervisorService
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
        public bool AuthorizeDevice(byte[] registerData)
        {
            return false;
        }

        /// <summary>
        /// The get authorization requests.
        /// </summary>
        /// <returns>
        /// List of authorization requests
        /// </returns>
        public IList<IAuthorizationRequest> GetAuthorizationRequests()
        {
            return new List<IAuthorizationRequest>();
        }

        /// <summary>
        /// The get path.
        /// </summary>
        /// <returns>
        /// The get path
        /// </returns>
        public string GetPath()
        {
            string virtualPath = HostingEnvironment.ApplicationVirtualPath;
            string absolutePath = OperationContext.Current.Channel.LocalAddress.Uri.AbsolutePath;
            string path = absolutePath.Substring(virtualPath.Length);

            return path;
        }

        #endregion
    }
}