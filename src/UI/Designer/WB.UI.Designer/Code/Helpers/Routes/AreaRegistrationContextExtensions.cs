// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AreaRegistrationContextExtensions.cs" company="">
//   
// </copyright>
// <summary>
//   Contains extension methods to map routes in Areas to lowercase URLs.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace WB.UI.Designer.Code.Helpers.Routes
{
    using System.Linq;
    using System.Web.Mvc;
    using System.Web.Routing;

    /// <summary>
    ///     Contains extension methods to map routes in Areas to lowercase URLs.
    /// </summary>
    public static class AreaRegistrationContextExtensions
    {
        #region Public Methods and Operators

        /// <summary>
        /// Maps the specified URL route using a lowercase URL and associates it with the area that is specified by the AreaName property. Does not change casing in the querystring, if any.
        /// </summary>
        /// <param name="context">
        /// The context that encapsulates the information that is required in order to register an area within an ASP.NET MVC application.
        /// </param>
        /// <param name="name">
        /// The name of the route.
        /// </param>
        /// <param name="url">
        /// The URL pattern for the route.
        /// </param>
        /// <returns>
        /// A reference to the mapped route.
        /// </returns>
        public static Route MapRouteLowercase(this AreaRegistrationContext context, string name, string url)
        {
            return MapRouteLowercase(context, name, url, null, null, null);
        }

        /// <summary>
        /// Maps the specified URL route using a lowercase URL and associates it with the area that is specified by the AreaName property, using the specified route default values. Does not change casing in the querystring, if any.
        /// </summary>
        /// <param name="context">
        /// The context that encapsulates the information that is required in order to register an area within an ASP.NET MVC application.
        /// </param>
        /// <param name="name">
        /// The name of the route.
        /// </param>
        /// <param name="url">
        /// The URL pattern for the route.
        /// </param>
        /// <param name="defaults">
        /// An object that contains default route values.
        /// </param>
        /// <returns>
        /// A reference to the mapped route.
        /// </returns>
        public static Route MapRouteLowercase(
            this AreaRegistrationContext context, string name, string url, object defaults)
        {
            return MapRouteLowercase(context, name, url, defaults, null, null);
        }

        /// <summary>
        /// Maps the specified URL route using a lowercase URL and associates it with the area that is specified by the AreaName property, using the specified namespaces. Does not change casing in the querystring, if any.
        /// </summary>
        /// <param name="context">
        /// The context that encapsulates the information that is required in order to register an area within an ASP.NET MVC application.
        /// </param>
        /// <param name="name">
        /// The name of the route.
        /// </param>
        /// <param name="url">
        /// The URL pattern for the route.
        /// </param>
        /// <param name="namespaces">
        /// A set of namespaces for the application.
        /// </param>
        /// <returns>
        /// A reference to the mapped route.
        /// </returns>
        public static Route MapRouteLowercase(
            this AreaRegistrationContext context, string name, string url, string[] namespaces)
        {
            return MapRouteLowercase(context, name, url, null, null, namespaces);
        }

        /// <summary>
        /// Maps the specified URL route using a lowercase URL and associates it with the area that is specified by the AreaName property, using the specified route default values and constraints. Does not change casing in the querystring, if any.
        /// </summary>
        /// <param name="context">
        /// The context that encapsulates the information that is required in order to register an area within an ASP.NET MVC application.
        /// </param>
        /// <param name="name">
        /// The name of the route.
        /// </param>
        /// <param name="url">
        /// The URL pattern for the route.
        /// </param>
        /// <param name="defaults">
        /// An object that contains default route values.
        /// </param>
        /// <param name="constraints">
        /// A set of expressions that specify valid values for a URL parameter.
        /// </param>
        /// <returns>
        /// A reference to the mapped route.
        /// </returns>
        public static Route MapRouteLowercase(
            this AreaRegistrationContext context, string name, string url, object defaults, object constraints)
        {
            return MapRouteLowercase(context, name, url, defaults, constraints, null);
        }

        /// <summary>
        /// Maps the specified URL route using a lowercase URL and associates it with the area that is specified by the AreaName property, using the specified route default values and namespaces. Does not change casing in the querystring, if any.
        /// </summary>
        /// <param name="context">
        /// The context that encapsulates the information that is required in order to register an area within an ASP.NET MVC application.
        /// </param>
        /// <param name="name">
        /// The name of the route.
        /// </param>
        /// <param name="url">
        /// The URL pattern for the route.
        /// </param>
        /// <param name="defaults">
        /// An object that contains default route values.
        /// </param>
        /// <param name="namespaces">
        /// A set of namespaces for the application.
        /// </param>
        /// <returns>
        /// A reference to the mapped route.
        /// </returns>
        public static Route MapRouteLowercase(
            this AreaRegistrationContext context, string name, string url, object defaults, string[] namespaces)
        {
            return MapRouteLowercase(context, name, url, defaults, null, namespaces);
        }

        /// <summary>
        /// Maps the specified URL route using a lowercase URL and associates it with the area that is specified by the AreaName property, using the specified route default values, constraints, and namespaces. Does not change casing in the querystring, if any.
        /// </summary>
        /// <param name="context">
        /// The context that encapsulates the information that is required in order to register an area within an ASP.NET MVC application.
        /// </param>
        /// <param name="name">
        /// The name of the route.
        /// </param>
        /// <param name="url">
        /// The URL pattern for the route.
        /// </param>
        /// <param name="defaults">
        /// An object that contains default route values.
        /// </param>
        /// <param name="constraints">
        /// A set of expressions that specify valid values for a URL parameter.
        /// </param>
        /// <param name="namespaces">
        /// A set of namespaces for the application.
        /// </param>
        /// <returns>
        /// A reference to the mapped route.
        /// </returns>
        public static Route MapRouteLowercase(
            this AreaRegistrationContext context, 
            string name, 
            string url, 
            object defaults, 
            object constraints, 
            string[] namespaces)
        {
            if (namespaces == null && context.Namespaces != null)
            {
                namespaces = context.Namespaces.ToArray();
            }

            Route route = context.Routes.MapRouteLowercase(name, url, defaults, constraints, namespaces);

            route.DataTokens["area"] = context.AreaName;
            route.DataTokens["UseNamespaceFallback"] = namespaces == null || namespaces.Length == 0;

            return route;
        }

        #endregion
    }
}