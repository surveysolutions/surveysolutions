using System;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Web;
using SynchronizationMessages.Discover;

namespace Web.Supervisor.WCF
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "SpotSyncService" in code, svc and config file together.
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class SpotSyncService : ISpotSync
    {
        

        public string Process()
        {
            if (OperationContext.Current.Host.BaseAddresses.Count == 0)
                return null;

            Uri uri = OperationContext.Current.Host.BaseAddresses[0];
            
           // return uri.Scheme + Uri.SchemeDelimiter + uri.Host + ":" + uri.Port;
            return uri.GetLeftPart(UriPartial.Authority) + HttpRuntime.AppDomainAppVirtualPath;
            //  return System.Environment.MachineName;
            //return HttpContext.Current.Server.MapPath(".");
        }
    }
}
