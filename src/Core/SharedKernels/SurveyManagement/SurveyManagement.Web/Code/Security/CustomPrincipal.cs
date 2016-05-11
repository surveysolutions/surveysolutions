using System;
using System.Security.Principal;
using System.Web.Security;
using WB.Core.SharedKernels.SurveyManagement.Web.Properties;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Code.Security
{
    public class CustomPrincipal : IPrincipal
    {
        private readonly CustomIdentity identity;

        public CustomPrincipal(CustomIdentity identity)
        {
            this.identity = identity;
        }

        public System.Security.Principal.IIdentity Identity
        {

            get { return identity; }

        }

        public bool IsInRole(string role)
        {
            if (this.identity == null)
                throw new Exception(Strings.CustomPrincipal_IdentityIsNullError);
            if (!this.identity.IsAuthenticated || role == null)
                return false;
            
            if (!Roles.Enabled)
                return false;

            return Roles.IsUserInRole(this.Identity.Name, role);
        }
        
    }
}