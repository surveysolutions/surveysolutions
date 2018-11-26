using WB.Core.GenericSubdomains.Portable;

namespace WB.UI.Designer
{
    using System.Web.Mvc;
    using System.Web.Mvc.Html;

    /// <summary>
    /// The custom html helper.
    /// </summary>
    public static class CustomHtmlHelper
    {
        #region Public Methods and Operators

        /// <summary>
        /// The if.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="evaluation">
        /// The evaluation.
        /// </param>
        /// <returns>
        /// The <see cref="MvcHtmlString"/>.
        /// </returns>
        public static MvcHtmlString If(this MvcHtmlString value, bool evaluation)
        {
            return evaluation ? value : MvcHtmlString.Empty;
        }

        /// <summary>
        /// The menu action link.
        /// </summary>
        /// <param name="helper">
        /// The helper.
        /// </param>
        /// <param name="title">
        /// The title.
        /// </param>
        /// <param name="action">
        /// The action.
        /// </param>
        /// <param name="controller">
        /// The controller.
        /// </param>
        /// <param name="role"></param>
        /// <returns>
        /// The <see cref="MvcHtmlString"/>.
        /// </returns>
        public static MvcHtmlString MenuActionLink(
            this HtmlHelper helper, string title, string action, string controller, string role = null)
        {
            var li = new TagBuilder("li");
            if (role != null) li.Attributes.Add("role", role);
            if (action.Compare(GlobalHelper.CurrentAction) && controller.Compare(GlobalHelper.CurrentController))
            {
                li.AddCssClass("active");
            }
            
            li.InnerHtml = helper.ActionLink(title, action, controller).ToHtmlString();

            return MvcHtmlString.Create(li.ToString());
        }

        #endregion
    }
}
