using System.Web.Mvc;

namespace WB.UI.Designer
{
    public static class CustomHtmlHelper
    {
        public static MvcHtmlString If(this MvcHtmlString value, bool evaluation)
        {
            return evaluation ? value : MvcHtmlString.Empty;
        }
    }
}