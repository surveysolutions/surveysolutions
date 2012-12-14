using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Web;
using Main.Core.Entities;
using Main.Core.WCF;

namespace Web.Supervisor.WCF
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "AuthorizationService" in code, svc and config file together.
    [ServiceBehaviorAttribute(IncludeExceptionDetailInFaults = true)]
    public class AuthorizationService : BaseAuthorizationService
    {
        protected override string OnGetPath()
        {
            var virtualPath = System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath;
            var absolutePath = OperationContext.Current.Channel.LocalAddress.Uri.AbsolutePath;
            var path = absolutePath.Substring(virtualPath.Length);

            return path;
        }
    }
}
