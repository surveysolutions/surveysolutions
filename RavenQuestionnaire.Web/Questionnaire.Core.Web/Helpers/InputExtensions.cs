namespace Questionnaire.Core.Web.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Routing;

    /// <summary>
    /// The input extensions.
    /// </summary>
    public static class InputExtensions
    {
        #region Public Methods and Operators

        /// <summary>
        /// The icon input.
        /// </summary>
        /// <param name="htmlHelper">
        /// The html helper.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="labelText">
        /// The label text.
        /// </param>
        /// <param name="inputHtmlAttributes">
        /// The input html attributes.
        /// </param>
        /// <param name="buttonAttributes">
        /// The button attributes.
        /// </param>
        /// <param name="labelAttributes">
        /// The label attributes.
        /// </param>
        /// <returns>
        /// The System.Web.Mvc.MvcHtmlString.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// </exception>
        public static MvcHtmlString IconInput(
            this HtmlHelper htmlHelper, 
            string name, 
            object value, 
            string labelText, 
            IDictionary<string, object> inputHtmlAttributes, 
            IDictionary<string, object> buttonAttributes, 
            IDictionary<string, object> labelAttributes)
        {
            string fullId = htmlHelper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldId(name);
            if (string.IsNullOrEmpty(fullId))
            {
                throw new ArgumentException("Name is empty");
            }

            var spanTagBuilder = new TagBuilder("span");
            spanTagBuilder.MergeAttributes(labelAttributes);
            spanTagBuilder.MergeAttribute("from", fullId, true);
            spanTagBuilder.SetInnerText(value.ToString());

            var aTagBuilder = new TagBuilder("a");
            aTagBuilder.MergeAttributes(buttonAttributes);
            aTagBuilder.MergeAttribute("open-virtual-keyboar", "true", true);
            aTagBuilder.MergeAttribute("href", "#", true);
            aTagBuilder.MergeAttribute(
                "style", "background-size:25px;height:25px;width:25px;position:absolute; left:-15px; top:15px;", true);
            aTagBuilder.MergeAttribute("grab-parent-areas", "5", false);
            aTagBuilder.MergeAttribute("target-input", fullId, true);
            aTagBuilder.SetInnerText( /*System.Web.HttpUtility.HtmlDecode("&nbsp;")*/ string.Empty);
            string required = string.Empty;
            if (!inputHtmlAttributes.ContainsKey("type"))
            {
                inputHtmlAttributes.Add("type", "text");
            }

            if (inputHtmlAttributes.ContainsKey("required")
                && !string.IsNullOrEmpty(inputHtmlAttributes["required"].ToString()))
            {
                required = "<span style=\"color: red;font-size:25px;\">*</span>";
                inputHtmlAttributes.Remove("required");
            }

            var additionTags =
                new MvcHtmlString(
                    string.Format(
                        "<div>{3}<p class=\"text-question\"><label for=\"{0}\" style=\"display:inline\">{1}  {5}</label>{2}</p>{4}<div style=\"clear: both;\"></div></div>", 
                        fullId, 
                        HttpUtility.HtmlDecode(labelText), 
                        spanTagBuilder.ToString(TagRenderMode.Normal), 
                        aTagBuilder.ToString(TagRenderMode.Normal), 
                        InputHelper(htmlHelper, name, value, true, true, inputHtmlAttributes), 
                        required));

            return additionTags;
        }

        /// <summary>
        /// The icon input.
        /// </summary>
        /// <param name="htmlHelper">
        /// The html helper.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="labelText">
        /// The label text.
        /// </param>
        /// <param name="inputHtmlAttributes">
        /// The input html attributes.
        /// </param>
        /// <param name="buttonAttributes">
        /// The button attributes.
        /// </param>
        /// <param name="labelAttributes">
        /// The label attributes.
        /// </param>
        /// <returns>
        /// The System.Web.Mvc.MvcHtmlString.
        /// </returns>
        public static MvcHtmlString IconInput(
            this HtmlHelper htmlHelper, 
            string name, 
            object value, 
            string labelText, 
            object inputHtmlAttributes, 
            object buttonAttributes, 
            object labelAttributes)
        {
            return IconInput(
                htmlHelper, 
                name, 
                value, 
                labelText, 
                HtmlHelper.AnonymousObjectToHtmlAttributes(inputHtmlAttributes), 
                HtmlHelper.AnonymousObjectToHtmlAttributes(buttonAttributes), 
                HtmlHelper.AnonymousObjectToHtmlAttributes(labelAttributes));
        }

        /// <summary>
        /// The icon input.
        /// </summary>
        /// <param name="htmlHelper">
        /// The html helper.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="labelText">
        /// The label text.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <returns>
        /// The System.Web.Mvc.MvcHtmlString.
        /// </returns>
        public static MvcHtmlString IconInput(this HtmlHelper htmlHelper, string name, string labelText, object value)
        {
            return IconInput(htmlHelper, name, value, labelText, null, null, (object)null);
        }

        /// <summary>
        /// The icon input.
        /// </summary>
        /// <param name="htmlHelper">
        /// The html helper.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="labelText">
        /// The label text.
        /// </param>
        /// <param name="htmlAttributes">
        /// The html attributes.
        /// </param>
        /// <returns>
        /// The System.Web.Mvc.MvcHtmlString.
        /// </returns>
        public static MvcHtmlString IconInput(
            this HtmlHelper htmlHelper, string name, object value, string labelText, object htmlAttributes)
        {
            return IconInput(htmlHelper, name, value, labelText, null, htmlAttributes, null);
        }

        /// <summary>
        /// The icon input comments.
        /// </summary>
        /// <param name="htmlHelper">
        /// The html helper.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="labelText">
        /// The label text.
        /// </param>
        /// <param name="inputHtmlAttributes">
        /// The input html attributes.
        /// </param>
        /// <param name="buttonAttributes">
        /// The button attributes.
        /// </param>
        /// <param name="labelAttributes">
        /// The label attributes.
        /// </param>
        /// <returns>
        /// The System.Web.Mvc.MvcHtmlString.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// </exception>
        public static MvcHtmlString IconInputComments(
            this HtmlHelper htmlHelper, 
            string name, 
            object value, 
            string labelText, 
            object inputHtmlAttributes, 
            object buttonAttributes, 
            object labelAttributes)
        {
            string fullId = htmlHelper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldId(name);
            if (string.IsNullOrEmpty(fullId))
            {
                throw new ArgumentException("Name is empty");
            }

            var spanTagBuilder = new TagBuilder("span");
            spanTagBuilder.MergeAttributes(HtmlHelper.AnonymousObjectToHtmlAttributes(labelAttributes));
            spanTagBuilder.MergeAttribute("from", fullId, true);
            spanTagBuilder.SetInnerText(value.ToString());

            var aTagBuilder = new TagBuilder("a");
            aTagBuilder.MergeAttributes(HtmlHelper.AnonymousObjectToHtmlAttributes(buttonAttributes));
            aTagBuilder.MergeAttribute("open-virtual-keyboar", "true", true);
            aTagBuilder.MergeAttribute("href", "#", true);
            aTagBuilder.MergeAttribute("target-input", fullId, true);
            aTagBuilder.SetInnerText(string.Empty);
            RouteValueDictionary attr = HtmlHelper.AnonymousObjectToHtmlAttributes(inputHtmlAttributes);
            if (!attr.ContainsKey("type"))
            {
                attr.Add("type", "text");
            }

            var additionTags =
                new MvcHtmlString(
                    string.Format(
                        "<div>{0}{1}</div>", 
                        aTagBuilder.ToString(TagRenderMode.Normal), 
                        InputHelper(htmlHelper, name, value, true, true, attr)));
            return additionTags;
        }

        #endregion

        // Helper methods
        #region Methods

        /// <summary>
        /// The input helper.
        /// </summary>
        /// <param name="htmlHelper">
        /// The html helper.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="setId">
        /// The set id.
        /// </param>
        /// <param name="isExplicitValue">
        /// The is explicit value.
        /// </param>
        /// <param name="htmlAttributes">
        /// The html attributes.
        /// </param>
        /// <returns>
        /// The System.String.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// </exception>
        private static string InputHelper(
            HtmlHelper htmlHelper, 
            string name, 
            object value, 
            bool setId, 
            bool isExplicitValue, 
            IDictionary<string, object> htmlAttributes)
        {
            string fullName = htmlHelper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(name);
            if (string.IsNullOrEmpty(fullName))
            {
                throw new ArgumentException("Name is empty");
            }

            var tagBuilder = new TagBuilder("input");
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

            return /*System.Web.HttpUtility.HtmlDecode(*/ tagBuilder.ToString(TagRenderMode.SelfClosing) /*)*/;
        }

        #endregion
    }
}