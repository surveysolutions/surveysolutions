using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.Security;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Code.Security
{
    public static class PrincipalReplacer
    {
        public static void ReplacePrincipal()
        {
            IPrincipal usr = HttpContext.Current.User;
            if (usr.Identity.IsAuthenticated && usr.Identity.AuthenticationType == "Forms")
            {
                FormsIdentity fIdent = usr.Identity as FormsIdentity;
                CustomIdentity ci = new CustomIdentity(fIdent.Ticket);
                CustomPrincipal customPrincipal = new CustomPrincipal(ci);
                HttpContext.Current.User = customPrincipal;
                Thread.CurrentPrincipal = customPrincipal;
            }
        }
    }
}
