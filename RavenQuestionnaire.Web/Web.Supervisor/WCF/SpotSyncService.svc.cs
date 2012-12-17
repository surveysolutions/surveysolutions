// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SpotSyncService.svc.cs" company="The World Bank">
//   Spot Sync Service
// </copyright>
// <summary>
//   The spot sync service.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Web.Supervisor.WCF
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.Web;

    using SynchronizationMessages.Discover;

    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "SpotSyncService" in code, svc and config file together.
   
    /// <summary>
    /// The spot sync service.
    /// </summary>
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class SpotSyncService : ISpotSync
    {
        #region Public Methods and Operators

        /// <summary>
        /// The process.
        /// </summary>
        /// <returns>
        /// The process
        /// </returns>
        public string Process()
        {
            if (OperationContext.Current.Host.BaseAddresses.Count == 0)
            {
                return null;
            }

            Uri uri = OperationContext.Current.Host.BaseAddresses[0];

            return uri.GetLeftPart(UriPartial.Authority) + HttpRuntime.AppDomainAppVirtualPath;
        }

        #endregion
    }
}