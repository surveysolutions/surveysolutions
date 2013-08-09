namespace Questionnaire.Core.Web.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Web.Mvc;
    using System.Web.Mvc.Html;

    using Main.Core.Entities.SubEntities;

    /// <summary>
    /// The select extensions.
    /// </summary>
    public static class SelectExtensions
    {
        #region Public Methods and Operators

        /// <summary>
        /// The enum drop down list for.
        /// </summary>
        /// <param name="htmlHelper">
        /// The html helper.
        /// </param>
        /// <param name="expression">
        /// The expression.
        /// </param>
        /// <typeparam name="TModel">
        /// </typeparam>
        /// <typeparam name="TProperty">
        /// </typeparam>
        /// <returns>
        /// The System.Web.Mvc.MvcHtmlString.
        /// </returns>
        public static MvcHtmlString EnumDropDownListFor<TModel, TProperty>(
            this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression) where TModel : class
            where TProperty : struct, IConvertible
        {
            TProperty value = GetValue(htmlHelper, expression);
            return htmlHelper.DropDownListFor(expression, ToSelectList(typeof(TProperty), value.ToString()));

            // return htmlHelper.DropDownListFor(expression, ToSelectList(typeof(TProperty), value.ToInt32(CultureInfo.InvariantCulture).ToString()));
        }

        /// <summary>
        /// The enum drop down list for.
        /// </summary>
        /// <param name="htmlHelper">
        /// The html helper.
        /// </param>
        /// <param name="expression">
        /// The expression.
        /// </param>
        /// <param name="htmlAttributes">
        /// The html attributes.
        /// </param>
        /// <typeparam name="TModel">
        /// </typeparam>
        /// <typeparam name="TProperty">
        /// </typeparam>
        /// <returns>
        /// The System.Web.Mvc.MvcHtmlString.
        /// </returns>
        public static MvcHtmlString EnumDropDownListFor<TModel, TProperty>(
            this HtmlHelper<TModel> htmlHelper, 
            Expression<Func<TModel, TProperty>> expression, 
            IDictionary<string, object> htmlAttributes) where TModel : class where TProperty : struct, IConvertible
        {
            TProperty value = GetValue(htmlHelper, expression);

            // return htmlHelper.DropDownListFor(expression, ToSelectList(typeof(TProperty), value.ToInt32(CultureInfo.InvariantCulture).ToString()), htmlAttributes);
            return htmlHelper.DropDownListFor(
                expression, ToSelectList(typeof(TProperty), value.ToString()), htmlAttributes);
        }

        /// <summary>
        /// The extended drop down list for.
        /// </summary>
        /// <param name="htmlHelper">
        /// The html helper.
        /// </param>
        /// <param name="expression">
        /// The expression.
        /// </param>
        /// <typeparam name="TModel">
        /// </typeparam>
        /// <typeparam name="TProperty">
        /// </typeparam>
        /// <returns>
        /// The System.Web.Mvc.MvcHtmlString.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// </exception>
        public static MvcHtmlString ExtendedDropDownListFor<TModel, TProperty>(
            this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression) where TModel : class
        {
            if (!typeof(TProperty).IsGenericType)
            {
                throw new ArgumentException("T must be an generic type");
            }

            TProperty value = htmlHelper.ViewData.Model == null
                                  ? default(TProperty)
                                  : expression.Compile()(htmlHelper.ViewData.Model);
            SelectList sl = ToSelectList(value as Dictionary<string, string>);
            MvcHtmlString mvcHtmlString = htmlHelper.DropDownListFor(expression, sl, sl.SelectedValue);
            return mvcHtmlString;
        }

        /// <summary>
        /// The to select list.
        /// </summary>
        /// <param name="enumType">
        /// The enum type.
        /// </param>
        /// <param name="selectedItem">
        /// The selected item.
        /// </param>
        /// <returns>
        /// The System.Collections.Generic.List`1[T -&gt; System.Web.Mvc.SelectListItem].
        /// </returns>
        public static List<SelectListItem> ToSelectList(Type enumType, string selectedItem)
        {
            var items = new List<SelectListItem>();
            foreach (object item in Enum.GetValues(enumType))
            {
                if (enumType != typeof(QuestionType) || (enumType == typeof(QuestionType) && item.ToString() != "YesNo"))
                {
                    FieldInfo fi = enumType.GetField(item.ToString());
                    object attribute = fi.GetCustomAttributes(typeof(DescriptionAttribute), true).FirstOrDefault();
                    string title = attribute == null ? item.ToString() : ((DescriptionAttribute)attribute).Description;
                    string value = item.ToString();
                    items.Add(new SelectListItem { Value = value, Text = title, Selected = value == selectedItem });
                }
            }

            return items;
        }

        /// <summary>
        /// The to select list.
        /// </summary>
        /// <param name="collection">
        /// The collection.
        /// </param>
        /// <returns>
        /// The System.Web.Mvc.SelectList.
        /// </returns>
        public static SelectList ToSelectList(Dictionary<string, string> collection)
        {
            return new SelectList(collection, "Key", "Value", collection.FirstOrDefault());
        }

        #endregion

        #region Methods

        /// <summary>
        /// The get value.
        /// </summary>
        /// <param name="htmlHelper">
        /// The html helper.
        /// </param>
        /// <param name="expression">
        /// The expression.
        /// </param>
        /// <typeparam name="TModel">
        /// </typeparam>
        /// <typeparam name="TProperty">
        /// </typeparam>
        /// <returns>
        /// The TProperty.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// </exception>
        private static TProperty GetValue<TModel, TProperty>(
            HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression)
        {
            if (!typeof(TProperty).IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type");
            }

            TProperty value = htmlHelper.ViewData.Model == null
                                  ? default(TProperty)
                                  : expression.Compile()(htmlHelper.ViewData.Model);
            return value;
        }

        #endregion
    }
}