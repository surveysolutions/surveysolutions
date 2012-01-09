using System;
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

        public static MvcHtmlString EnumDropDownListFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression)
            where TModel : class
            where TProperty : struct, IConvertible
        {
            if (!typeof(TProperty).IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type");
            }
            var value = htmlHelper.ViewData.Model == null
                ? default(TProperty)
                : expression.Compile()(htmlHelper.ViewData.Model);

            return htmlHelper.DropDownListFor(expression, ToSelectList(typeof(TProperty), value.ToInt32(CultureInfo.InvariantCulture).ToString()));
        }

        public static SelectList ToSelectList(Type enumType, string selectedItem)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            foreach (var item in Enum.GetValues(enumType))
            {
                FieldInfo fi = enumType.GetField(item.ToString());
                var attribute = fi.GetCustomAttributes(typeof(DescriptionAttribute), true).FirstOrDefault();
                var title = attribute == null ? item.ToString() : ((DescriptionAttribute)attribute).Description;
                var listItem = new SelectListItem
                {
                    Value = ((int)item).ToString(),
                    Text = title
                };
                items.Add(listItem);
            }

            return new SelectList(items, "Value", "Text", selectedItem);
        }
    }
}
