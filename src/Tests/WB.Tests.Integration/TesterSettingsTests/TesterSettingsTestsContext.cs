using System;
using System.Web.Http;

namespace WB.Tests.Integration.TesterSettingsTests
{
    internal class TesterSettingsTestsContext
    {
        public static string GetRoutePrefix(Type controllerType)
        {
            var routePrefixAttribute = (RoutePrefixAttribute)Attribute.GetCustomAttribute(controllerType, typeof(RoutePrefixAttribute));

            return routePrefixAttribute.Prefix;
        }
    }
}
