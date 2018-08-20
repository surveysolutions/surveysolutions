using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Filters
{
    public class TypedDirectRouteProvider : DefaultDirectRouteProvider
    {
        internal static readonly Dictionary<Type, Dictionary<string, TypedRoute>> Routes = new Dictionary<Type, Dictionary<string, TypedRoute>>();

        protected override IReadOnlyList<IDirectRouteFactory> GetActionRouteFactories(HttpActionDescriptor actionDescriptor)
        {
            var factories = base.GetActionRouteFactories(actionDescriptor).ToList();
            if (Routes.ContainsKey(actionDescriptor.ControllerDescriptor.ControllerType))
            {
                var controllerLevelDictionary = Routes[actionDescriptor.ControllerDescriptor.ControllerType];
                if (controllerLevelDictionary.ContainsKey(actionDescriptor.ActionName))
                {
                    factories.Add(controllerLevelDictionary[actionDescriptor.ActionName]);
                }
            }

            return factories;
        }
    }

    public static class Param
    {
        public static TValue Any<TValue>()
        {
            return default(TValue);
        }
    }

    public static class HttpConfigurationExtensions
    {
        public static TypedRoute TypedRoute(this HttpConfiguration config, string template, Action<TypedRoute> configSetup)
        {
            var route = new TypedRoute(template);
            configSetup(route);

            if (TypedDirectRouteProvider.Routes.ContainsKey(route.ControllerType))
            {
                var controllerLevelDictionary = TypedDirectRouteProvider.Routes[route.ControllerType];
                if (!controllerLevelDictionary.ContainsKey(route.ActionName))
                    controllerLevelDictionary.Add(route.ActionName, route);
            }
            else
            {
                var controllerLevelDictionary = new Dictionary<string, TypedRoute> { { route.ActionName, route } };
                TypedDirectRouteProvider.Routes.Add(route.ControllerType, controllerLevelDictionary);
            }

            return route;
        }
    }

    public class TypedRoute : IDirectRouteFactory
    {
        public TypedRoute(string template)
        {
            Template = template;
        }

        public Type ControllerType { get; private set; }

        public string RouteName { get; private set; }

        public string Template { get; private set; }

        public string ControllerName => this.ControllerType != null ? this.ControllerType.FullName : string.Empty;

        public string ActionName { get; private set; }

        public MethodInfo ActionMember { get; private set; }

        RouteEntry IDirectRouteFactory.CreateRoute(DirectRouteFactoryContext context)
        {
            IDirectRouteBuilder builder = context.CreateBuilder(Template);

            builder.Name = RouteName;
            return builder.Build();
        }

        public TypedRoute Controller<TController>() where TController : IHttpController
        {
            ControllerType = typeof(TController);
            return this;
        }

        public TypedRoute Action<T, U>(Expression<Func<T, U>> expression)
        {
            ActionMember = GetMethodInfoInternal(expression);
            ControllerType = typeof(T);
            ActionName = ActionMember.Name;
            return this;
        }

        public TypedRoute Action<T>(Expression<Action<T>> expression)
        {
            ActionMember = GetMethodInfoInternal(expression);
            ControllerType = typeof(T);
            ActionName = ActionMember.Name;
            return this;
        }

        private static MethodInfo GetMethodInfoInternal(dynamic expression)
        {
            var method = expression.Body as MethodCallExpression;
            if (method != null)
                return method.Method;

            throw new ArgumentException("Expression is incorrect!");
        }

        public TypedRoute Name(string name)
        {
            RouteName = name;
            return this;
        }

        public TypedRoute Action(string actionName)
        {
            ActionName = actionName;
            return this;
        }
    }
}
