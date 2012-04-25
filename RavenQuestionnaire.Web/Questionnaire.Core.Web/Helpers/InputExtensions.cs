using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using System.Web.Routing;
using System.Web.Mvc.Html;
namespace Questionnaire.Core.Web.Helpers
{
    public static class InputExtensions
    {
        public static MvcHtmlString IconInput(this HtmlHelper htmlHelper, string name, object value, IDictionary<string, object> inputHtmlAttributes, IDictionary<string, object> htmlAttributes)
        {
            string fullId = htmlHelper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldId(name);
            if (String.IsNullOrEmpty(fullId))
            {
                throw new ArgumentException("Name is empty");
            }
            TagBuilder spanTagBuilder = new TagBuilder("span");
            spanTagBuilder.MergeAttribute("for", fullId, true);
            spanTagBuilder.SetInnerText(value.ToString());

            TagBuilder aTagBuilder = new TagBuilder("a");
            aTagBuilder.MergeAttributes(htmlAttributes);
            aTagBuilder.MergeAttribute("open-virtual-keyboar", "true", true);
            aTagBuilder.MergeAttribute("href", "javascript:void(0)", true);
            aTagBuilder.MergeAttribute("target-input", fullId, true);
            aTagBuilder.SetInnerText(string.Empty);
            if(!inputHtmlAttributes.ContainsKey("type"))
            {
                inputHtmlAttributes.Add("type", "text");
            }
            var additionTags =
                new MvcHtmlString(spanTagBuilder.ToString(TagRenderMode.Normal) +
                                  aTagBuilder.ToString(TagRenderMode.Normal) +
                                  InputHelper(htmlHelper, name, value, true, true, inputHtmlAttributes));
            return additionTags;

        }

        public static MvcHtmlString IconInput(this HtmlHelper htmlHelper, string name, object value, object inputHtmlAttributes, object htmlAttributes)
        {
            return IconInput(htmlHelper, name, value, HtmlHelper.AnonymousObjectToHtmlAttributes(inputHtmlAttributes), HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }
        public static MvcHtmlString IconInput(this HtmlHelper htmlHelper, string name, object value)
        {
            return IconInput(htmlHelper, name, value, (object)null, (object)null);
        }
        public static MvcHtmlString IconInput(this HtmlHelper htmlHelper, string name, object value, object htmlAttributes)
        {
            return IconInput(htmlHelper, name, value, (object)null, htmlAttributes);
        }

        #region private
        // Helper methods

        private static string InputHelper(HtmlHelper htmlHelper, string name, object value, bool setId, bool isExplicitValue, IDictionary<string, object> htmlAttributes)
        {
            string fullName = htmlHelper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(name);
            if (String.IsNullOrEmpty(fullName))
            {
                throw new ArgumentException("Name is empty");
            }

            TagBuilder tagBuilder = new TagBuilder("input");
            tagBuilder.MergeAttributes(htmlAttributes);
            tagBuilder.MergeAttribute("name", fullName, true);

            string valueParameter = Convert.ToString(value, CultureInfo.CurrentCulture);

            tagBuilder.MergeAttribute("value", valueParameter, isExplicitValue);

            if (setId)
            {
                tagBuilder.GenerateId(fullName);
            }

            // If there are any errors for a named field, we add the css attribute.
            ModelState modelState;
            if (htmlHelper.ViewData.ModelState.TryGetValue(fullName, out modelState))
            {
                if (modelState.Errors.Count > 0)
                {
                    tagBuilder.AddCssClass(HtmlHelper.ValidationInputCssClassName);
                }
            }


            return /*System.Web.HttpUtility.HtmlDecode(*/tagBuilder.ToString(TagRenderMode.SelfClosing)/*)*/;
        }
        #endregion
    }
}
