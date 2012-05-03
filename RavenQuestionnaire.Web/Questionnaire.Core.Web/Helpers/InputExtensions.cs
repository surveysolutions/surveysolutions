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
        public static MvcHtmlString IconInput(this HtmlHelper htmlHelper, string name, object value, string labelText, IDictionary<string, object> inputHtmlAttributes, IDictionary<string, object> buttonAttributes, IDictionary<string, object> labelAttributes)
        {
            string fullId = htmlHelper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldId(name);
            if (String.IsNullOrEmpty(fullId))
            {
                throw new ArgumentException("Name is empty");
            }
            TagBuilder spanTagBuilder = new TagBuilder("span");
            spanTagBuilder.MergeAttributes(labelAttributes);
            spanTagBuilder.MergeAttribute("from", fullId, true);
            spanTagBuilder.SetInnerText(value.ToString());

            TagBuilder aTagBuilder = new TagBuilder("a");
            aTagBuilder.MergeAttributes(buttonAttributes);
            aTagBuilder.MergeAttribute("open-virtual-keyboar", "true", true);
            aTagBuilder.MergeAttribute("href", "#", true);
            aTagBuilder.MergeAttribute("target-input", fullId, true);
            aTagBuilder.SetInnerText(/*System.Web.HttpUtility.HtmlDecode("&nbsp;")*/ string.Empty);
            if(!inputHtmlAttributes.ContainsKey("type"))
            {
                inputHtmlAttributes.Add("type", "text");
            }
            var additionTags =
                new MvcHtmlString(string.Format("<label for=\"{0}\" >{1}",fullId,labelText)+aTagBuilder.ToString(TagRenderMode.Normal) +"</label>"+
                                  spanTagBuilder.ToString(TagRenderMode.Normal) +
                                  InputHelper(htmlHelper, name, value, true, true, inputHtmlAttributes));
            return additionTags;

        }

        public static MvcHtmlString IconInput(this HtmlHelper htmlHelper, string name, object value, string labelText, object inputHtmlAttributes, object buttonAttributes, object labelAttributes)
        {
            return IconInput(htmlHelper, name, value, labelText,
                             HtmlHelper.AnonymousObjectToHtmlAttributes(inputHtmlAttributes),
                             HtmlHelper.AnonymousObjectToHtmlAttributes(buttonAttributes),
                             HtmlHelper.AnonymousObjectToHtmlAttributes(labelAttributes));
        }
        public static MvcHtmlString IconInput(this HtmlHelper htmlHelper, string name, string labelText, object value)
        {
            return IconInput(htmlHelper, name, value, labelText, (object)null, (object)null, (object)null);
        }
        public static MvcHtmlString IconInput(this HtmlHelper htmlHelper, string name, object value, string labelText, object htmlAttributes)
        {
            return IconInput(htmlHelper, name, value, labelText, (object)null, htmlAttributes, (object)null);
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
