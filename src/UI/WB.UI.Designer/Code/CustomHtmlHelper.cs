using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using WB.Core.GenericSubdomains.Portable;
using WB.UI.Designer.Code;

namespace WB.UI.Designer
{
    public static class CustomHtmlHelper
    {
        public static IHtmlContent MenuActionLink(
            this IHtmlHelper helper, ViewContext viewContext, string title, string action, string controller, string role = null)
        {
            var li = new TagBuilder("li");
            if (role != null) li.Attributes.Add("role", role);
            if (action.Compare(viewContext.CurrentAction()) && controller.Compare(viewContext.CurrentController()))
            {
                li.AddCssClass("active");
            }
            
            li.InnerHtml.AppendHtml(helper.ActionLink(title, action, controller));

            return new HtmlString(li.ToString());
        }
    }
}
