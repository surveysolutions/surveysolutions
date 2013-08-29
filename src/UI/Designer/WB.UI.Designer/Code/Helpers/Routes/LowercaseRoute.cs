namespace WB.UI.Designer.Code.Helpers.Routes
{
    using System.Web.Routing;

    /// <summary>
    /// The lowercase route.
    /// </summary>
    internal class LowercaseRoute : Route
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LowercaseRoute"/> class.
        /// </summary>
        /// <param name="url">
        /// The url.
        /// </param>
        /// <param name="routeHandler">
        /// The route handler.
        /// </param>
        public LowercaseRoute(string url, IRouteHandler routeHandler)
            : base(url, routeHandler)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LowercaseRoute"/> class.
        /// </summary>
        /// <param name="url">
        /// The url.
        /// </param>
        /// <param name="defaults">
        /// The defaults.
        /// </param>
        /// <param name="routeHandler">
        /// The route handler.
        /// </param>
        public LowercaseRoute(string url, RouteValueDictionary defaults, IRouteHandler routeHandler)
            : base(url, defaults, routeHandler)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LowercaseRoute"/> class.
        /// </summary>
        /// <param name="url">
        /// The url.
        /// </param>
        /// <param name="defaults">
        /// The defaults.
        /// </param>
        /// <param name="constraints">
        /// The constraints.
        /// </param>
        /// <param name="routeHandler">
        /// The route handler.
        /// </param>
        public LowercaseRoute(
            string url, RouteValueDictionary defaults, RouteValueDictionary constraints, IRouteHandler routeHandler)
            : base(url, defaults, constraints, routeHandler)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LowercaseRoute"/> class.
        /// </summary>
        /// <param name="url">
        /// The url.
        /// </param>
        /// <param name="defaults">
        /// The defaults.
        /// </param>
        /// <param name="constraints">
        /// The constraints.
        /// </param>
        /// <param name="dataTokens">
        /// The data tokens.
        /// </param>
        /// <param name="routeHandler">
        /// The route handler.
        /// </param>
        public LowercaseRoute(
            string url, 
            RouteValueDictionary defaults, 
            RouteValueDictionary constraints, 
            RouteValueDictionary dataTokens, 
            IRouteHandler routeHandler)
            : base(url, defaults, constraints, dataTokens, routeHandler)
        {
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The get virtual path.
        /// </summary>
        /// <param name="requestContext">
        /// The request context.
        /// </param>
        /// <param name="values">
        /// The values.
        /// </param>
        /// <returns>
        /// The <see cref="VirtualPathData"/>.
        /// </returns>
        public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary values)
        {
            VirtualPathData path = base.GetVirtualPath(requestContext, values);

            if (path != null)
            {
                string virtualPath = path.VirtualPath;
                int lastIndexOf = virtualPath.LastIndexOf("?");

                if (lastIndexOf != 0)
                {
                    if (lastIndexOf > 0)
                    {
                        string leftPart = virtualPath.Substring(0, lastIndexOf).ToLowerInvariant();
                        string queryPart = virtualPath.Substring(lastIndexOf);
                        path.VirtualPath = leftPart + queryPart;
                    }
                    else
                    {
                        path.VirtualPath = path.VirtualPath.ToLowerInvariant();
                    }
                }
            }

            return path;
        }

        #endregion
    }
}