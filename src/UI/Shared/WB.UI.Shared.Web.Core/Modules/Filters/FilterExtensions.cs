using System;
using System.Linq;
using System.Web.Http.Controllers;
using System.Web.Http.Services;

namespace WB.UI.Shared.Web.Modules.Filters
{
    public class FilterExtensions
    {
        public static bool HasMarkerAttribute(HttpActionDescriptor actionDescriptor, Type attributeType)
        {
            ReflectedHttpActionDescriptor reflectedActionDescriptor;

            // Check whether the ActionDescriptor is wrapped in a decorator or not.
            if (actionDescriptor is IDecorator<HttpActionDescriptor> wrapper)
            {
                reflectedActionDescriptor = wrapper.Inner as ReflectedHttpActionDescriptor;
            }
            else
            {
                reflectedActionDescriptor = actionDescriptor as ReflectedHttpActionDescriptor;
            }

            if (reflectedActionDescriptor != null)
            {
                return reflectedActionDescriptor
                    .MethodInfo
                    .GetCustomAttributes(attributeType, false)
                    .Any();
            }

            throw new ArgumentException("Cant get attributes for action");
        }

        public static bool HasMarkerAttribute(HttpControllerDescriptor controllerDescriptor, Type attributeType)
        {
            return controllerDescriptor.ControllerType.GetCustomAttributes(attributeType, false).Any();
        }

        public static bool HasActionOrControllerMarkerAttribute(HttpActionDescriptor actionDescriptor, Type attributeType)
        {
            var hasActionAttributes = HasMarkerAttribute(actionDescriptor, attributeType);
            var hasControllerAttributes = HasMarkerAttribute(actionDescriptor.ControllerDescriptor, attributeType);
            return hasActionAttributes || hasControllerAttributes;
        }
    }
}
