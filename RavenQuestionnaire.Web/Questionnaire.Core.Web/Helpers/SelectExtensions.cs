using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace Questionnaire.Core.Web.Helpers
{
    public static class SelectExtensions
    {

        #region Public

        public static MvcHtmlString ExtendedDropDownListFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression)
            where TModel : class
        {
            if (!typeof(TProperty).IsGenericType)
            {
                throw new ArgumentException("T must be an generic type");
            }
            var value = htmlHelper.ViewData.Model == null
                ? default(TProperty)
                : expression.Compile()(htmlHelper.ViewData.Model);
            SelectList sl = ToSelectList(value as Dictionary<string, string>);
            MvcHtmlString mvcHtmlString = htmlHelper.DropDownListFor(expression, sl, sl.SelectedValue);
            return mvcHtmlString;
        }

        public static MvcHtmlString EnumDropDownListFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression)
            where TModel : class
            where TProperty : struct, IConvertible
        {
            var value = GetValue(htmlHelper, expression);
            return htmlHelper.DropDownListFor(expression, ToSelectList(typeof(TProperty), value.ToInt32(CultureInfo.InvariantCulture).ToString()));
        }

        public static MvcHtmlString EnumDropDownListFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, IDictionary<string, object> htmlAttributes)
            where TModel : class
            where TProperty : struct, IConvertible
        {
            var value = GetValue(htmlHelper, expression);
            return htmlHelper.DropDownListFor(expression, ToSelectList(typeof(TProperty), value.ToInt32(CultureInfo.InvariantCulture).ToString()), htmlAttributes);
        }

        public static List<SelectListItem> ToSelectList(Type enumType, string selectedItem)
        {
            var items = new List<SelectListItem>();
            foreach (var item in Enum.GetValues(enumType))
            {
                FieldInfo fi = enumType.GetField(item.ToString());
                var attribute = fi.GetCustomAttributes(typeof (DescriptionAttribute), true).FirstOrDefault();
                var title = attribute == null ? item.ToString() : ((DescriptionAttribute) attribute).Description;
                var value = (item).ToString();
                items.Add(new SelectListItem{Value = value, Text = title, Selected = (value == selectedItem) });
            }

            return items;
        }

        public static SelectList ToSelectList(Dictionary<string, string> collection)
        {
            return new SelectList(collection, "Key", "Value", collection.FirstOrDefault());
        }

        #endregion

        #region Private

        private static TProperty GetValue<TModel, TProperty>(HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression)
        {
            if (!typeof(TProperty).IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type");
            }
            var value = htmlHelper.ViewData.Model == null
                ? default(TProperty)
                : expression.Compile()(htmlHelper.ViewData.Model);
            return value;
        }

        #endregion
        
    }

}
