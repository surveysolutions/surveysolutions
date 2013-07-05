using System.Diagnostics.CodeAnalysis;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using System.Web.Mvc.Html;
using System.Web.Routing;
using Main.Core.Entities.SubEntities;

namespace Questionnaire.Core.Web.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public static class GridFormExtensions
    {

        /// <summary>
        /// The begin form html 5.
        /// </summary>
        /// <param name="ajaxHelper">
        /// The ajax helper.
        /// </param>
        /// <param name="actionName">
        /// The action name.
        /// </param>
        /// <param name="ajaxOptions">
        /// The ajax options.
        /// </param>
        /// <returns>
        /// The System.Web.Mvc.Html.MvcForm.
        /// </returns>
        public static MvcForm BeginGridFormHtml5(this AjaxHelper ajaxHelper, string actionName, AjaxOptions ajaxOptions, Guid questionnaireid, Guid questionId, QuestionType questionType)
        {
            return BeginGridFormHtml5(ajaxHelper, actionName, (string)null /* controllerName */, ajaxOptions, questionnaireid, questionId, questionType);
        }

        /// <summary>
        /// The begin form html 5.
        /// </summary>
        /// <param name="ajaxHelper">
        /// The ajax helper.
        /// </param>
        /// <param name="actionName">
        /// The action name.
        /// </param>
        /// <param name="routeValues">
        /// The route values.
        /// </param>
        /// <param name="ajaxOptions">
        /// The ajax options.
        /// </param>
        /// <returns>
        /// The System.Web.Mvc.Html.MvcForm.
        /// </returns>
        public static MvcForm BeginGridFormHtml5(
            this AjaxHelper ajaxHelper, string actionName, object routeValues, AjaxOptions ajaxOptions, Guid questionnaireid, Guid questionId, QuestionType questionType)
        {
            return BeginGridFormHtml5(ajaxHelper, actionName, null /* controllerName */, routeValues, ajaxOptions, questionnaireid, questionId, questionType);
        }

        /// <summary>
        /// The begin form html 5.
        /// </summary>
        /// <param name="ajaxHelper">
        /// The ajax helper.
        /// </param>
        /// <param name="actionName">
        /// The action name.
        /// </param>
        /// <param name="routeValues">
        /// The route values.
        /// </param>
        /// <param name="ajaxOptions">
        /// The ajax options.
        /// </param>
        /// <param name="htmlAttributes">
        /// The html attributes.
        /// </param>
        /// <returns>
        /// The System.Web.Mvc.Html.MvcForm.
        /// </returns>
        public static MvcForm BeginGridFormHtml5(
            this AjaxHelper ajaxHelper,
            string actionName,
            object routeValues,
            AjaxOptions ajaxOptions,
            object htmlAttributes, Guid questionnaireid, Guid questionId, QuestionType questionType)
        {
            return BeginGridFormHtml5(
                ajaxHelper, actionName, null /* controllerName */, routeValues, ajaxOptions, htmlAttributes, questionnaireid, questionId, questionType);
        }

        /// <summary>
        /// The begin form html 5.
        /// </summary>
        /// <param name="ajaxHelper">
        /// The ajax helper.
        /// </param>
        /// <param name="actionName">
        /// The action name.
        /// </param>
        /// <param name="routeValues">
        /// The route values.
        /// </param>
        /// <param name="ajaxOptions">
        /// The ajax options.
        /// </param>
        /// <returns>
        /// The System.Web.Mvc.Html.MvcForm.
        /// </returns>
        public static MvcForm BeginGridFormHtml5(
            this AjaxHelper ajaxHelper, string actionName, RouteValueDictionary routeValues, AjaxOptions ajaxOptions, Guid questionnaireid, Guid questionId, QuestionType questionType)
        {
            return BeginGridFormHtml5(ajaxHelper, actionName, null /* controllerName */, routeValues, ajaxOptions, questionnaireid, questionId, questionType);
        }

        /// <summary>
        /// The begin form html 5.
        /// </summary>
        /// <param name="ajaxHelper">
        /// The ajax helper.
        /// </param>
        /// <param name="actionName">
        /// The action name.
        /// </param>
        /// <param name="routeValues">
        /// The route values.
        /// </param>
        /// <param name="ajaxOptions">
        /// The ajax options.
        /// </param>
        /// <param name="htmlAttributes">
        /// The html attributes.
        /// </param>
        /// <returns>
        /// The System.Web.Mvc.Html.MvcForm.
        /// </returns>
        public static MvcForm BeginGridFormHtml5(
            this AjaxHelper ajaxHelper,
            string actionName,
            RouteValueDictionary routeValues,
            AjaxOptions ajaxOptions,
            IDictionary<string, object> htmlAttributes, Guid questionnaireid, Guid questionId, QuestionType questionType)
        {
            return BeginGridFormHtml5(
                ajaxHelper, actionName, null /* controllerName */, routeValues, ajaxOptions, htmlAttributes, questionnaireid, questionId, questionType);
        }

        /// <summary>
        /// The begin form html 5.
        /// </summary>
        /// <param name="ajaxHelper">
        /// The ajax helper.
        /// </param>
        /// <param name="actionName">
        /// The action name.
        /// </param>
        /// <param name="controllerName">
        /// The controller name.
        /// </param>
        /// <param name="ajaxOptions">
        /// The ajax options.
        /// </param>
        /// <returns>
        /// The System.Web.Mvc.Html.MvcForm.
        /// </returns>
        public static MvcForm BeginGridFormHtml5(
            this AjaxHelper ajaxHelper, string actionName, string controllerName, AjaxOptions ajaxOptions, Guid questionnaireid, Guid questionId, QuestionType questionType)
        {
            return BeginGridFormHtml5(
                ajaxHelper, actionName, controllerName, null /* values */, ajaxOptions, null /* htmlAttributes */, questionnaireid, questionId, questionType);
        }
        public static MvcForm BeginGridFormHtml5(
            this AjaxHelper ajaxHelper, string actionName, string controllerName, AjaxOptions ajaxOptions, Guid questionnaireid, Guid questionId, string formName)
        {
            return BeginGridFormHtml5(
                ajaxHelper, actionName, controllerName, null /* values */, ajaxOptions, null /* htmlAttributes */,
                questionnaireid, questionId, "Comments", formName);
        }
        /// <summary>
        /// The begin form html 5.
        /// </summary>
        /// <param name="ajaxHelper">
        /// The ajax helper.
        /// </param>
        /// <param name="actionName">
        /// The action name.
        /// </param>
        /// <param name="controllerName">
        /// The controller name.
        /// </param>
        /// <param name="routeValues">
        /// The route values.
        /// </param>
        /// <param name="ajaxOptions">
        /// The ajax options.
        /// </param>
        /// <returns>
        /// The System.Web.Mvc.Html.MvcForm.
        /// </returns>
        public static MvcForm BeginGridFormHtml5(
            this AjaxHelper ajaxHelper,
            string actionName,
            string controllerName,
            object routeValues,
            AjaxOptions ajaxOptions, Guid questionnaireid, Guid questionId, QuestionType questionType)
        {
            return BeginGridFormHtml5(
                ajaxHelper, actionName, controllerName, routeValues, ajaxOptions, null /* htmlAttributes */, questionnaireid, questionId, questionType);
        }

        /// <summary>
        /// The begin form html 5.
        /// </summary>
        /// <param name="ajaxHelper">
        /// The ajax helper.
        /// </param>
        /// <param name="actionName">
        /// The action name.
        /// </param>
        /// <param name="controllerName">
        /// The controller name.
        /// </param>
        /// <param name="routeValues">
        /// The route values.
        /// </param>
        /// <param name="ajaxOptions">
        /// The ajax options.
        /// </param>
        /// <param name="htmlAttributes">
        /// The html attributes.
        /// </param>
        /// <returns>
        /// The System.Web.Mvc.Html.MvcForm.
        /// </returns>
        public static MvcForm BeginGridFormHtml5(
            this AjaxHelper ajaxHelper,
            string actionName,
            string controllerName,
            object routeValues,
            AjaxOptions ajaxOptions,
            object htmlAttributes, Guid questionnaireid, Guid questionId, QuestionType questionType)
        {
            var newValues = new RouteValueDictionary(routeValues);
            RouteValueDictionary newAttributes = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
            return BeginGridFormHtml5(
                ajaxHelper, actionName, controllerName, newValues, ajaxOptions, newAttributes, questionnaireid, questionId, questionType.ToString(), null);

        }

        /// <summary>
        /// The begin form html 5.
        /// </summary>
        /// <param name="ajaxHelper">
        /// The ajax helper.
        /// </param>
        /// <param name="actionName">
        /// The action name.
        /// </param>
        /// <param name="controllerName">
        /// The controller name.
        /// </param>
        /// <param name="routeValues">
        /// The route values.
        /// </param>
        /// <param name="ajaxOptions">
        /// The ajax options.
        /// </param>
        /// <returns>
        /// The System.Web.Mvc.Html.MvcForm.
        /// </returns>
        public static MvcForm BeginGridFormHtml5(
            this AjaxHelper ajaxHelper,
            string actionName,
            string controllerName,
            RouteValueDictionary routeValues,
            AjaxOptions ajaxOptions, Guid questionnaireid, Guid questionId, QuestionType questionType, string formName)
        {

            return BeginGridFormHtml5(
                ajaxHelper, actionName, controllerName, routeValues, ajaxOptions, null, questionnaireid, questionId,
                questionType.ToString(), null);
        }

        public static MvcForm BeginGridFormHtml5(
            this AjaxHelper ajaxHelper,
            string actionName,
            string controllerName,
            RouteValueDictionary routeValues,
            AjaxOptions ajaxOptions,
            IDictionary<string, object> htmlAttributes, Guid questionnaireid, Guid questionId, string handlerType, string formName)
        {
            if (htmlAttributes == null)
                htmlAttributes = new Dictionary<string, object>();
            htmlAttributes.Add(string.IsNullOrEmpty(formName) ? "answer-form" : formName, questionId);
            var result = Html5Extensions.BeginFormHtml5(
                ajaxHelper, actionName, controllerName, routeValues, ajaxOptions, htmlAttributes);

            ajaxHelper.ViewContext.Writer.Write(
                string.Format("<input type='hidden' value='{0}' name='QuestionnaireId' />", questionnaireid));
            ajaxHelper.ViewContext.Writer.Write(
                string.Format("<input type='hidden' value='{0}' name='PublicKey' />", questionId));
            ajaxHelper.ViewContext.Writer.Write(
                string.Format("<input type='hidden' value='{0}' name='QuestionType' />", handlerType));

            ajaxHelper.ViewContext.Writer.Write("<input type='hidden' value='' name='PropogationPublicKey' />");
            return result;
        }
    }
}
