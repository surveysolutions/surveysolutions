using System.Web.Mvc;
using WB.UI.Shared.Web.Settings;

namespace WB.UI.Shared.Web.Filters
{
    public class RequireSecureConnectionAttribute : RequireHttpsAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (!CoreSettings.IsHttpsRequired)
            {
                return;
            }

            base.OnAuthorization(filterContext);
        }

    }
}