// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ViewHelperExtensions.cs" company="">
//   
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Web.Supervisor.Utils
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Web.Mvc;
    using System.Web.Mvc.Html;
    using System.Web.Routing;

    /// <summary>
    /// The default scaffolding extensions.
    /// </summary>
    public static class DefaultScaffoldingExtensions
    {
        #region Public Methods and Operators

        /// <summary>
        /// The get action name.
        /// </summary>
        /// <param name="actionExpression">
        /// The action expression.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string GetActionName(this LambdaExpression actionExpression)
        {
            return ((MethodCallExpression)actionExpression.Body).Method.Name;
        }

        /// <summary>
        /// The get controller name.
        /// </summary>
        /// <param name="controllerType">
        /// The controller type.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string GetControllerName(this Type controllerType)
        {
            return controllerType.Name.Replace("Controller", string.Empty);
        }

        /// <summary>
        /// The get id.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <returns>
        /// The <see cref="object"/>.
        /// </returns>
        public static object GetId(this object model)
        {
            return model.GetType().GetProperty(model.IdentifierPropertyName()).GetValue(model, new object[0]);
        }

        /// <summary>
        /// The get id value.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <returns>
        /// The <see cref="RouteValueDictionary"/>.
        /// </returns>
        public static RouteValueDictionary GetIdValue(this object model)
        {
            var v = new RouteValueDictionary();
            v.Add(model.IdentifierPropertyName(), model.GetId());
            return v;
        }

        /// <summary>
        /// The get label.
        /// </summary>
        /// <param name="propertyInfo">
        /// The property info.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string GetLabel(this PropertyInfo propertyInfo)
        {
            ModelMetadata meta = ModelMetadataProviders.Current.GetMetadataForProperty(
                null, propertyInfo.DeclaringType, propertyInfo.Name);
            return meta.GetDisplayName();
        }

        /// <summary>
        /// The identifier property name.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string IdentifierPropertyName(this object model)
        {
            return IdentifierPropertyName(model.GetType());
        }

        /// <summary>
        /// The identifier property name.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string IdentifierPropertyName(this Type type)
        {
            if (type.GetProperties().Any(info => info.AttributeExists<KeyAttribute>()))
            {
                return type.GetProperties().First(info => info.AttributeExists<KeyAttribute>()).Name;
            }
            else if (type.GetProperties().Any(p => p.Name.Equals("id", StringComparison.CurrentCultureIgnoreCase)))
            {
                return
                    type.GetProperties().First(p => p.Name.Equals("id", StringComparison.CurrentCultureIgnoreCase)).Name;
            }
            else if (
                type.GetProperties()
                    .Any(p => p.Name.Equals(type.Name + "id", StringComparison.CurrentCultureIgnoreCase)))
            {
                return
                    type.GetProperties()
                        .First(p => p.Name.Equals(type.Name + "id", StringComparison.CurrentCultureIgnoreCase))
                        .Name;
            }

            return string.Empty;
        }

        /// <summary>
        /// The ordered by display attr.
        /// </summary>
        /// <param name="collection">
        /// The collection.
        /// </param>
        /// <returns>
        /// The <see cref="IOrderedEnumerable"/>.
        /// </returns>
        public static IOrderedEnumerable<PropertyInfo> OrderedByDisplayAttr(this IEnumerable<PropertyInfo> collection)
        {
            return collection.OrderBy(
                col =>
                    {
                        var attr = col.GetAttribute<DisplayAttribute>();
                        return (attr != null ? attr.GetOrder() : null) ?? 0;
                    });
        }

        /// <summary>
        /// The to separated words.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string ToSeparatedWords(this string value)
        {
            return Regex.Replace(value, "([A-Z][a-z])", " $1").Trim();
        }

        /// <summary>
        /// The visible properties.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <returns>
        /// The <see cref="PropertyInfo[]"/>.
        /// </returns>
        public static PropertyInfo[] VisibleProperties(this object model)
        {
            return
                model.GetType()
                     .GetProperties()
                     .Where(info => info.Name != model.IdentifierPropertyName())
                     .OrderedByDisplayAttr()
                     .ToArray();
        }

        #endregion
    }

    /// <summary>
    /// The property info extensions.
    /// </summary>
    public static class PropertyInfoExtensions
    {
        #region Public Methods and Operators

        /// <summary>
        /// The attribute exists.
        /// </summary>
        /// <param name="propertyInfo">
        /// The property info.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool AttributeExists<T>(this PropertyInfo propertyInfo) where T : class
        {
            var attribute = propertyInfo.GetCustomAttributes(typeof(T), false).FirstOrDefault() as T;
            if (attribute == null)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// The attribute exists.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool AttributeExists<T>(this Type type) where T : class
        {
            var attribute = type.GetCustomAttributes(typeof(T), false).FirstOrDefault() as T;
            if (attribute == null)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// The get attribute.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="T"/>.
        /// </returns>
        public static T GetAttribute<T>(this Type type) where T : class
        {
            return type.GetCustomAttributes(typeof(T), false).FirstOrDefault() as T;
        }

        /// <summary>
        /// The get attribute.
        /// </summary>
        /// <param name="propertyInfo">
        /// The property info.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="T"/>.
        /// </returns>
        public static T GetAttribute<T>(this PropertyInfo propertyInfo) where T : class
        {
            return propertyInfo.GetCustomAttributes(typeof(T), false).FirstOrDefault() as T;
        }

        /// <summary>
        /// The get label.
        /// </summary>
        /// <param name="Model">
        /// The model.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string GetLabel(this object Model)
        {
            return LabelFromType(Model.GetType());
        }

        /// <summary>
        /// The get label.
        /// </summary>
        /// <param name="Model">
        /// The model.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string GetLabel(this IEnumerable Model)
        {
            Type elementType = Model.GetType().GetElementType();
            if (elementType == null)
            {
                elementType = Model.GetType().GetGenericArguments()[0];
            }

            return LabelFromType(elementType);
        }

        /// <summary>
        /// The label from type.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string LabelFromType(Type @type)
        {
            var att = GetAttribute<DisplayNameAttribute>(@type);
            return att != null ? att.DisplayName : @type.Name.ToSeparatedWords();
        }

        #endregion
    }

    /// <summary>
    /// The html helper extensions.
    /// </summary>
    public static class HtmlHelperExtensions
    {
        #region Public Methods and Operators

        /// <summary>
        /// The try partial.
        /// </summary>
        /// <param name="helper">
        /// The helper.
        /// </param>
        /// <param name="viewName">
        /// The view name.
        /// </param>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <returns>
        /// The <see cref="MvcHtmlString"/>.
        /// </returns>
        public static MvcHtmlString TryPartial(this HtmlHelper helper, string viewName, object model)
        {
            try
            {
                return helper.Partial(viewName, model);
            }
            catch (Exception)
            {
            }

            return MvcHtmlString.Empty;
        }

        #endregion
    }
}