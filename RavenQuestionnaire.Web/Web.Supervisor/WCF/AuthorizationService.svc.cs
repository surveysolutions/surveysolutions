// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AuthorizationService.svc.cs" company="">
//   
// </copyright>
// <summary>
//   The authorization service.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Web.Supervisor.WCF
{
    using System.ServiceModel;
    using System.Web.Hosting;

    using Main.Core.WCF;

    /// <summary>
    /// The authorization service.
    /// </summary>
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "AuthorizationService" in code, svc and config file together.
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public class AuthorizationService : BaseAuthorizationService
    {
        #region Methods

        /// <summary>
        /// The on get path.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        protected override string OnGetPath()
        {
            string virtualPath = HostingEnvironment.ApplicationVirtualPath;
            string absolutePath = OperationContext.Current.Channel.LocalAddress.Uri.AbsolutePath;
            string path = absolutePath.Substring(virtualPath.Length);

            return path;
        }

        #endregion
    }
}